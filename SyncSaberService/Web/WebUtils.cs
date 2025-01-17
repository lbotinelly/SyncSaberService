﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using System.Net;

namespace SyncSaberService.Web
{
    public static class HttpClientWrapper
    {
        private static bool _initialized = false;
        private static object lockObject = new object();
        private static HttpClientHandler _httpClientHandler;
        public static HttpClientHandler httpClientHandler
        {
            get
            {
                if (_httpClientHandler == null)
                {
                    _httpClientHandler = new HttpClientHandler();
                    httpClientHandler.MaxConnectionsPerServer = 10;
                    httpClientHandler.UseCookies = true;
                }
                return _httpClientHandler;
            }
        }
        private static HttpClient _httpClient;
        public static HttpClient httpClient
        {
            get
            {
                lock (lockObject)
                {
                    if (_httpClient == null)
                    {
                        _httpClient = new HttpClient(httpClientHandler);
                        lock (_httpClient)
                        {
                            _httpClient.Timeout = new TimeSpan(0, 0, 10);
                        }
                    }
                }
                return _httpClient;
            }
        }

        public static void Initialize(int maxConnectionsPerServer)
        {
            if (_initialized == false)
            {
                _initialized = true;
                httpClientHandler.MaxConnectionsPerServer = maxConnectionsPerServer;
                httpClientHandler.UseCookies = true;
                _httpClient = new HttpClient(httpClientHandler);
            }
        }

        /// <summary>
        /// Downloads the page and returns it as a string.
        /// </summary>
        /// <param name="url"></param>
        /// <exception cref="HttpRequestException"></exception>
        /// <returns></returns>
        public static string GetPageText(string url)
        {
            Task<string> pageReadTask;
            //lock (lockObject)
            pageReadTask = httpClient.GetStringAsync(url);
            pageReadTask.Wait();
            string pageText = pageReadTask.Result;
            //Logger.Debug(pageText.Result);
            return pageText;
        }

        /// <summary>
        /// Downloads the page and returns it as a string in an asynchronous operation.
        /// </summary>
        /// <param name="url"></param>
        /// <exception cref="HttpRequestException"></exception>
        /// <returns></returns>
        public static async Task<string> GetPageTextAsync(string url)
        {
            //lock (lockObject)
            
            string pageText = await httpClient.GetStringAsync(url);
            //Logger.Debug(pageText.Result);
            //Logger.Debug($"Got page text for {url}");
            return pageText;
        }

        public static void AddCookies(CookieContainer newCookies, Uri uri)
        {
            lock(httpClientHandler)
            {
                if (httpClientHandler.CookieContainer == null)
                    httpClientHandler.CookieContainer = newCookies;
                else
                    httpClientHandler.CookieContainer.Add(newCookies.GetCookies(uri));
            }
        }

        public static void AddCookies(CookieContainer newCookies, string url)
        {
            AddCookies(newCookies, new Uri(url));
        }
    }

    // From https://stackoverflow.com/questions/45711428/download-file-with-webclient-or-httpclient
    public static class HttpContentExtensions
    {
        public static Task ReadAsFileAsync(this HttpContent content, string filename, bool overwrite)
        {
            string pathname = Path.GetFullPath(filename);
            if (!overwrite && File.Exists(filename))
            {
                throw new InvalidOperationException(string.Format("File {0} already exists.", pathname));
            }

            FileStream fileStream = null;
            try
            {
                fileStream = new FileStream(pathname, FileMode.Create, FileAccess.Write, FileShare.None);
                return content.CopyToAsync(fileStream).ContinueWith(
                    (copyTask) => {
                        fileStream.Close();
                    });
            }
            catch
            {
                if (fileStream != null)
                {
                    fileStream.Close();
                }

                throw;
            }
        }
    }
}
