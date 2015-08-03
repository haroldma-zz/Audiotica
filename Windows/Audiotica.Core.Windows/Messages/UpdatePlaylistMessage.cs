//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using System.Collections.Generic;
using Audiotica.Database.Models;

namespace Audiotica.Core.Windows.Messages
{
    public class UpdatePlaylistMessage
    {
        public List<Track> Songs;

        public UpdatePlaylistMessage(List<Track> songs)
        {
            Songs = songs;
        }
    }
}