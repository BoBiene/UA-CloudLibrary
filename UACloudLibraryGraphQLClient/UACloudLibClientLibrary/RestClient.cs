﻿/* ========================================================================
 * Copyright (c) 2005-2021 The OPC Foundation, Inc. All rights reserved.
 *
 * OPC Foundation MIT License 1.00
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 *
 * The complete license agreement can be found here:
 * http://opcfoundation.org/License/MIT/1.00/
 * ======================================================================*/

namespace UACloudLibClientLibrary
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using UACloudLibClientLibrary.Models;
    using UACloudLibrary;

    /// <summary>
    /// For use when the provider doesn't have a GraphQL interface and the downloading of nodesets
    /// </summary>
    internal class RestClient : IDisposable
    {
        private HttpClient client;

        public AuthenticationHeaderValue Authentication
        {
            set => client.DefaultRequestHeaders.Authorization = value;
            get => client.DefaultRequestHeaders.Authorization;
        }

        public RestClient(Uri address)
        {
            client = new HttpClient();
            client.BaseAddress = address;
        }

        public RestClient(string address, AuthenticationHeaderValue authentication)
        {
            client = new HttpClient();
            client.BaseAddress = new Uri(address);
            client.DefaultRequestHeaders.Authorization = authentication;
        }

        public void Dispose()
        {
            client.Dispose();
        }

        public async Task<List<AddressSpace>> GetBasicAddressSpaces(List<string> keywords = null)
        {
            if (keywords == null)
            {
                keywords = new List<string>() { "*" };
            }
            
            // keywords are simply appended with "&keywords=UriEscapedKeyword2&keywords=UriEscapedKeyword3", etc.)
            string address = client.BaseAddress.ToString() + "infomodel/find" + PrepareArgumentsString(keywords);

            HttpResponseMessage response = await client.GetAsync(address);
            List<BasicNodesetInformation> info = null;
            
            if(response.StatusCode == HttpStatusCode.OK)
            {
                info = JsonConvert.DeserializeObject<List<BasicNodesetInformation>>(await response.Content.ReadAsStringAsync());
            }

            return ConvertToAddressSpace(info);
        }

        public async Task<AddressSpace> DownloadNodeset(string identifier)
        {
            string address = Path.Combine(client.BaseAddress.ToString(), "infomodel/download/", Uri.EscapeDataString(identifier));
            HttpResponseMessage response = await client.GetAsync(address);
            AddressSpace resultType = null;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                resultType = JsonConvert.DeserializeObject<AddressSpace>(await response.Content.ReadAsStringAsync());
            }

            return resultType;
        }

        private List<AddressSpace> ConvertToAddressSpace(List<BasicNodesetInformation> Info)
        {
            List<AddressSpace> result = new List<AddressSpace>();
            if (Info != null)
            {
                foreach (BasicNodesetInformation basicNodesetInformation in Info)
                {
                    AddressSpace address = new AddressSpace();
                    address.Title = basicNodesetInformation.Title;
                    address.Version = basicNodesetInformation.Version;
                    address.Contributor.Name = basicNodesetInformation.Organisation;
                    address.License = basicNodesetInformation.License;
                    address.Nodeset.PublicationDate = basicNodesetInformation.CreationTime;
                    result.Add(address);
                }
            }

            return result;
        }

        private static string PrepareArgumentsString(List<string> arguments)
        {
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < arguments.Count; i++)
            {
                if (i == 0)
                {
                    stringBuilder.Append("?");
                }
                else
                {
                    stringBuilder.Append("&");
                }

                stringBuilder.Append("keywords=" + Uri.EscapeDataString(arguments[i]));
            }

            return stringBuilder.ToString();
        }
    }
}
