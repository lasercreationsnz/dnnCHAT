﻿/*
' Copyright (c) 2022 Christoc.com Software Solutions
'  All rights reserved.
' 
' Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
' documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
' the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, 
' and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
' 
' The above copyright notice and this permission notice shall be included in all copies or substantial portions 
' of the Software.
' 
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT 
' SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN 
' ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE 
' OR OTHER DEALINGS IN THE SOFTWARE.
' 
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DotNetNuke.Data;
using DotNetNuke.Framework.Providers;

namespace Christoc.Modules.DnnChat.Components
{

    /*
     * This class provides the DAL2 access to the storing of Connections within the DnnChat module
     */

    public class ConnectionRecordRoomController
    {
        private const string ProviderType = "data";
        private readonly ProviderConfiguration _providerConfiguration = ProviderConfiguration.GetProviderConfiguration(ProviderType);
        private readonly string _objectQualifier;
        private readonly string _databaseOwner;

        public ConnectionRecordRoomController()
        {
            var objProvider = (Provider)(_providerConfiguration.Providers[_providerConfiguration.DefaultProvider]);

            _objectQualifier = objProvider.Attributes["objectQualifier"];
            if (!string.IsNullOrEmpty(_objectQualifier) && _objectQualifier.EndsWith("_", StringComparison.Ordinal) == false)
            {
                _objectQualifier += "_";
            }

            _databaseOwner = objProvider.Attributes["databaseOwner"];
            if (!string.IsNullOrEmpty(_databaseOwner) && _databaseOwner.EndsWith(".", StringComparison.Ordinal) == false)
            {
                _databaseOwner += ".";
            }
        }

        public void CreateConnectionRecordRoom(ConnectionRecordRoom t)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<ConnectionRecordRoom>();
                rep.Insert(t);
            }
        }

        //TODO: This fails if there is no record
        public IEnumerable<ConnectionRecordRoom> GetConnectionRecordRooms(int moduleId)
        {
            IEnumerable<ConnectionRecordRoom> t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<ConnectionRecordRoom>();
                t = rep.Get(moduleId);
            }
            return t;
        }

        //method not used, fails for some reason
        public ConnectionRecordRoom GetConnectionRecordRoom(int id, Guid roomId)
        {
            try
            {
                ConnectionRecordRoom t;
                using (IDataContext ctx = DataContext.Instance())
                {
                    var rep = ctx.GetRepository<ConnectionRecordRoom>();
                    //TODO: something is erroring here "value can't be null"
                    t = rep.GetById(id, roomId);
                }
                return t;
            }
            catch (Exception exc)
            {
                return null;
            }
        }



        public ConnectionRecordRoom GetConnectionRecordRoomByConnectionRecordId(int connectionRecordId, Guid roomId)
        {
            try
            {
                ConnectionRecordRoom t;
                using (IDataContext ctx = DataContext.Instance())
                {
                    var rooms = ctx.ExecuteQuery<ConnectionRecordRoom>(CommandType.Text,
                                                           string.Format(
                                                               "select top 1 * from {0}{1}DnnChat_ConnectionRecordRooms where ConnectionRecordId = '{2}' and RoomId = '{3}'",
                                                               _databaseOwner,
                                                               _objectQualifier,
                                                              connectionRecordId, roomId)).ToList();

                    if (rooms.Any())
                    {
                        t = rooms[0];
                    }
                    else
                        return null;
                }
                return t;


            }
            catch (Exception exc)
            {
                return null;
            }
        }

        //get a list of the connectionrecordrooms
        public IEnumerable<Room> GetConnectionRecordRoomsByUserId(int userId)
        {
            IEnumerable<Room> t;
            using (IDataContext ctx = DataContext.Instance())
            {
                //TODO: update this SQL to only choose the LATEST connectionrecord, not all
                var connectionRecordRooms = ctx.ExecuteQuery<Room>(CommandType.Text,
                                                       string.Format(
                                                           "select distinct r.* from {0}{1}DnnChat_ConnectionRecordRooms crr join {0}{1}DnnChat_ConnectionRecords cr on (cr.ConnectionRecordId = crr.ConnectionRecordId)" +
                                                           " join {0}{1}DnnChat_Rooms r on (r.RoomId = crr.RoomId)" +
                                                           " where cr.UserId = '{2}' and crr.DepartedDate is null " +
                                                           " and r.enabled=1 and r.private = 0" +
                                                           " and cr.ConnectionRecordId in (select top 1 ConnectionRecordId from {0}{1}DnnChat_ConnectionRecords where  {1}DnnChat_ConnectionRecords.UserId ='{2}' order by {1}DnnChat_ConnectionRecords.ConnectionRecordId desc) " +
                                                           " order by r.roomName asc ",
                                                           _databaseOwner,
                                                           _objectQualifier,
                                                          userId)).ToList();

                if (connectionRecordRooms.Any())
                {
                    //get all rooms from the list of ConnectionRecords
                    t = connectionRecordRooms;
                }
                else
                    return null;
            }
            return t;
        }

        public void UpdateConnectionRecordRoom(ConnectionRecordRoom t)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<ConnectionRecordRoom>();
                rep.Update(t);
            }
        }

    }
}