﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RADProject.DataDomain.DTO;

namespace WebAPIClient
{
    public enum AUTHSTATUS { NONE, OK, INVALID, FAILED }

    public static class UserAuthentication
    {
        static public string baseWebAddress;
        static public string UserToken = "";
        static public AUTHSTATUS UserStatus = AUTHSTATUS.NONE;
        static public string IgdbUserToken = "PUT YOUR EXTERNAL WEB API TOKEN HERE"; // You'll need to ignup for your own Token Here

        // Example Endpoint api/GameScores/getTops/Count/
        static public List<T> getList<T>(string endpoint)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", UserToken);
                var response = client.GetAsync(baseWebAddress + endpoint).Result;
                var resultContent = response.Content.ReadAsAsync<List<T>>(
                    new[] { new JsonMediaTypeFormatter() }).Result;
                return resultContent;
            }
        }

        static public UserProfile getUserProfile()
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", UserToken);
                var response = client.GetAsync(baseWebAddress + "api/admin/userInfo").Result;
                var resultContent = response.Content.ReadAsAsync<UserProfile>(
                    new[] { new JsonMediaTypeFormatter() }).Result;
                return resultContent;
            }
        }

        static public bool PostModuleToStudent(StudentModuleDTO s)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = client.PostAsJsonAsync(baseWebAddress + "api/admin/assignStudentToModule", s).Result;
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Response Object is " + response.Content.ReadAsAsync<StudentModuleDTO>(
                        new[] { new JsonMediaTypeFormatter() }).Result.ToString());
                    return true;
                }
                return false;
            }
        }

        static public bool PostAssignmentToStudent(StudentModuleDTO s)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = client.PostAsJsonAsync(baseWebAddress + "api/admin/assignAssignmentToStudent", s).Result;
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Response Object is " + response.Content.ReadAsAsync<StudentModuleDTO>(
                        new[] { new JsonMediaTypeFormatter() }).Result.ToString());
                    return true;
                }
                return false;
            }
        }

        static public bool PostModuleToLecturer(LecturerModuleDTO l)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = client.PostAsJsonAsync(baseWebAddress + "api/admin/assignLecturerToModule", l).Result;
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Response Object is " + response.Content.ReadAsAsync<StudentModuleDTO>(
                        new[] { new JsonMediaTypeFormatter() }).Result.ToString());
                    return true;
                }
                return false;
            }
        }

        static public dynamic getExtModule(int moduleID)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("user-key", IgdbUserToken);
                var response = client.GetAsync("https://api-endpoint.igdb.com/modules/" + moduleID.ToString()).Result;
                var resultContent = response.Content.ReadAsAsync<JToken>(
                    new[] { new JsonMediaTypeFormatter() }).Result;
                var mname = resultContent.Children()["name"].Values<string>().FirstOrDefault();
                var msummary = resultContent.Children()["summary"].Values<string>().FirstOrDefault();
                // url is nested in cover object
                var mcover = resultContent.Children()["cover"]["url"].Values<string>().FirstOrDefault();
                var eobj =
                                        new
                                        {
                                            Name = mname,
                                            Summary = msummary,
                                            Cover = mcover
                                        };
                return eobj;
            }
        }

        static public dynamic getExtAssignment(int assignmentID)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("user-key", IgdbUserToken);
                var response = client.GetAsync("https://api-endpoint.igdb.com/modules/" + assignmentID.ToString()).Result;
                var resultContent = response.Content.ReadAsAsync<JToken>(
                    new[] { new JsonMediaTypeFormatter() }).Result;
                var aname = resultContent.Children()["name"].Values<string>().FirstOrDefault();
                var asummary = resultContent.Children()["summary"].Values<string>().FirstOrDefault();
                // url is nested in cover object
                var acover = resultContent.Children()["cover"]["url"].Values<string>().FirstOrDefault();
                var eobj =
                                        new
                                        {
                                            Name = aname,
                                            Summary = asummary,
                                            Cover = acover
                                        };
                return eobj;
            }
        }

        static public bool login(string username, string password)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("grant_type", "password"),
                        new KeyValuePair<string, string>("username", username),
                        new KeyValuePair<string, string>("password", password),
                    });
                var result = client.PostAsync(baseWebAddress + "Token", content).Result;
                try
                {
                    var resultContent = result.Content.ReadAsAsync<Token>(
                        new[] { new JsonMediaTypeFormatter() }
                        ).Result;
                    string ServerError = string.Empty;
                    if (!(String.IsNullOrEmpty(resultContent.AccessToken)))
                    {
                        Console.WriteLine(resultContent.AccessToken);
                        UserToken = resultContent.AccessToken;
                        UserStatus = AUTHSTATUS.OK;
                        return true;
                    }
                    else
                    {
                        UserToken = "Invalid Login";
                        UserStatus = AUTHSTATUS.INVALID;
                        Console.WriteLine("Invalid credentials");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    UserStatus = AUTHSTATUS.FAILED;
                    UserToken = "Server Error -> " + ex.Message;
                    Console.WriteLine(ex.Message);
                    return false;
                }

            }
        }
    }
}
