using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace ConfigCop
{
    class EndpointMethods
    {
        public static void GetBrowseObjects()
        {
            Program.BrowseList = new List<BrowseObj>();
            string browseFile = HelperMethods.CheckConfiguration("browseFile");

            if (File.Exists(browseFile))
            {
                using (StreamReader sr = new StreamReader(browseFile))
                {
                    string line = sr.ReadLine();
                    while (line != null)
                    {
                        string[] parts = line.Split('|');
                        string a = parts[0];
                        string b = "";
                        if (parts.Count() > 1)
                        {
                            b = parts[1];
                        }

                        Program.BrowseList.Add(new BrowseObj
                        {
                            Configured = a,
                            ActualUrl = b
                        });

                        line = sr.ReadLine();
                    }
                }
            }
        }

        public static void CheckEndpoint(string line, int lineNumber)
        {
            string[] protocols = HelperMethods.CheckConfiguration("protocols").Split(',');
            foreach (string p in protocols)
            {
                if (line.ToUpper().Contains(p.ToUpper()))
                {
                    TestConnection(line, lineNumber, p);
                }
            }
        }

        private static void TestConnection(string line, int lineNumber, string p)
        {
            string endpoint = ExtractEndpoint(line, p);

            if (IgnoreMethods.Ignore(endpoint) == false)
            {
                TestedConnection tc = HelperMethods.ConnectionTested(endpoint);
                if (tc != null)
                {
                    Errors.ConnectivityError(lineNumber, endpoint, tc.Reason, tc.StatusCode, tc.ConType);
                }
                else
                {
                    try
                    {
                        WebRequest request = WebRequest.Create(endpoint);
                        request.UseDefaultCredentials = true;
                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                        int sc;
                        string reason;
                        switch (response.StatusCode)
                        {
                            case HttpStatusCode.OK:
                                sc = Errors.GetStatusCode("HttpStatusCode.OK");
                                reason = "Request succeeded.  HTTP status 200.";
                                ReportConnectivityError(lineNumber, endpoint, sc, reason);
                                break;
                            case HttpStatusCode.Accepted:
                                sc = Errors.GetStatusCode("HttpStatusCode.Accepted");
                                reason = "Request has been accepted for further processing.  HTTP status 202.";
                                ReportConnectivityError(lineNumber, endpoint, sc, reason);
                                break;
                            case HttpStatusCode.Continue:
                                sc = Errors.GetStatusCode("HttpStatusCode.Continue");
                                reason = "Client can continue with request.  HTTP status 100.";
                                ReportConnectivityError(lineNumber, endpoint, sc, reason);
                                break;
                            case HttpStatusCode.Created:
                                sc = Errors.GetStatusCode("HttpStatusCode.Created");
                                reason = "New resource was created before the response was sent.  HTTP status 201.";
                                ReportConnectivityError(lineNumber, endpoint, sc, reason);
                                break;
                            case HttpStatusCode.Redirect:
                                sc = Errors.GetStatusCode("HttpStatusCode.Redirect");
                                reason = "Requested information was found but redirected.  HTTP status 302.";
                                ReportConnectivityError(lineNumber, endpoint, sc, reason);
                                break;
                            case HttpStatusCode.Unauthorized:
                                sc = Errors.GetStatusCode("HttpStatusCode.Unauthorized");
                                reason = Environment.UserName + " isn't authorized to access this url.  HTTP status 401.";
                                ReportConnectivityError(lineNumber, endpoint, sc, reason);
                                break;
                            case HttpStatusCode.ServiceUnavailable:
                                sc = Errors.GetStatusCode("HttpStatusCode.ServiceUnavailable");
                                reason = "Service Unavailable.  HTTP status 503.";
                                ReportConnectivityError(lineNumber, endpoint, sc, reason);
                                break;
                            case HttpStatusCode.RequestUriTooLong:
                                sc = Errors.GetStatusCode("HttpStatusCode.RequestUriTooLong");
                                reason = "Url is too long.  HTTP status 414.";
                                ReportConnectivityError(lineNumber, endpoint, sc, reason);
                                break;
                            case HttpStatusCode.RequestEntityTooLarge:
                                sc = Errors.GetStatusCode("HttpStatusCode.RequestEntityTooLarge");
                                reason = "Request entity is too large.  HTTP status 413.";
                                ReportConnectivityError(lineNumber, endpoint, sc, reason);
                                break;
                            case HttpStatusCode.ProxyAuthenticationRequired:
                                sc = Errors.GetStatusCode("HttpStatusCode.ProxyAuthenticationRequired");
                                reason = "Authentication is required.  Manually verify that " + Environment.UserName + " can access this endpoint.  HTTP status 407.";
                                ReportConnectivityError(lineNumber, endpoint, sc, reason);
                                break;
                            case HttpStatusCode.Moved:
                                sc = Errors.GetStatusCode("HttpStatusCode.Moved");
                                reason = "The endpoint has moved.  Contact NOG for the correct url.  HTTP status 301.";
                                ReportConnectivityError(lineNumber, endpoint, sc, reason);
                                break;
                            case HttpStatusCode.Ambiguous:
                                sc = Errors.GetStatusCode("HttpStatusCode.Ambiguous");
                                reason = "The request has multiple representtions.  Setting the defualt document for this endpoint may resolve the issue.  HTTP status 300.";
                                ReportConnectivityError(lineNumber, endpoint, sc, reason);
                                break;
                            case HttpStatusCode.BadGateway:
                                sc = Errors.GetStatusCode("HttpStatusCode.BadGateway");
                                reason = "Bad Gateway.  HTTP status 502.";
                                ReportConnectivityError(lineNumber, endpoint, sc, reason);
                                break;
                            case HttpStatusCode.BadRequest:
                                sc = Errors.GetStatusCode("HttpStatusCode.BadRequest");
                                reason = "Bad request.  HTTP status 400.";
                                ReportConnectivityError(lineNumber, endpoint, sc, reason);
                                break;
                            case HttpStatusCode.Conflict:
                                sc = Errors.GetStatusCode("HttpStatusCode.Conflict");
                                reason = "Conflict on the server.  HTTP status 409.";
                                ReportConnectivityError(lineNumber, endpoint, sc, reason);
                                break;
                            case HttpStatusCode.ExpectationFailed:
                                sc = Errors.GetStatusCode("HttpStatusCode.ExpectationFailed");
                                reason = "Expectation given in the Expect header could not be met by the server.  HTTP status 417.";
                                ReportConnectivityError(lineNumber, endpoint, sc, reason);
                                break;
                            case HttpStatusCode.Forbidden:
                                sc = Errors.GetStatusCode("HttpStatusCode.Forbidden");
                                reason = "Forbidden.  HTTP status 403.";
                                ReportConnectivityError(lineNumber, endpoint, sc, reason);
                                break;
                            case HttpStatusCode.GatewayTimeout:
                                sc = Errors.GetStatusCode("HttpStatusCode.GatewayTimeout");
                                reason = "Proxy server timed out while waiting for a response.  HTTP status 504.";
                                ReportConnectivityError(lineNumber, endpoint, sc, reason);
                                break;
                            case HttpStatusCode.Gone:
                                sc = Errors.GetStatusCode("HttpStatusCode.Gone");
                                reason = "Requested resource is no longer available.  HTTP status 410.";
                                ReportConnectivityError(lineNumber, endpoint, sc, reason);
                                break;
                            case HttpStatusCode.HttpVersionNotSupported:
                                sc = Errors.GetStatusCode("HttpStatusCode.HttpVersionNotSupported");
                                reason = "HTTP version not supported.  HTTP status 505.";
                                ReportConnectivityError(lineNumber, endpoint, sc, reason);
                                break;
                            case HttpStatusCode.InternalServerError:
                                sc = Errors.GetStatusCode("HttpStatusCode.InternalServerError");
                                reason = "Internal server error.  HTTP status 500.";
                                ReportConnectivityError(lineNumber, endpoint, sc, reason);
                                break;
                            case HttpStatusCode.LengthRequired:
                                sc = Errors.GetStatusCode("HttpStatusCode.LengthRequired");
                                reason = "Content Length header is missing.  HTTP status 411.";
                                ReportConnectivityError(lineNumber, endpoint, sc, reason);
                                break;
                            case HttpStatusCode.MethodNotAllowed:
                                sc = Errors.GetStatusCode("HttpStatusCode.MethodNotAllowed");
                                reason = "Request method not allowed.  HTTP status 405.";
                                ReportConnectivityError(lineNumber, endpoint, sc, reason);
                                break;
                            case HttpStatusCode.NoContent:
                                sc = Errors.GetStatusCode("HttpStatusCode.NoContent");
                                reason = "Response is intentionally blank.  HTTP status 204.";
                                ReportConnectivityError(lineNumber, endpoint, sc, reason);
                                break;
                            case HttpStatusCode.NonAuthoritativeInformation:
                                sc = Errors.GetStatusCode("HttpStatusCode.NonAuthoritativeInformation");
                                reason = "Response may be from a chached copy and not from the origin server.  HTTP status 203.";
                                ReportConnectivityError(lineNumber, endpoint, sc, reason);
                                break;
                            case HttpStatusCode.NotAcceptable:
                                sc = Errors.GetStatusCode("HttpStatusCode.NotAcceptable");
                                reason = "Client's Accept headers will not accept any available representations of the resource.  HTTP status 406.";
                                ReportConnectivityError(lineNumber, endpoint, sc, reason);
                                break;
                            case HttpStatusCode.NotFound:
                                sc = Errors.GetStatusCode("HttpStatusCode.NotFound");
                                reason = "Requested resource could not be found.  HTTP status 404.";
                                ReportConnectivityError(lineNumber, endpoint, sc, reason);
                                break;
                            case HttpStatusCode.NotImplemented:
                                sc = Errors.GetStatusCode("HttpStatusCode.NotImplemented");
                                reason = "Server does not support the requested function.  HTTP status 501.";
                                ReportConnectivityError(lineNumber, endpoint, sc, reason);
                                break;
                            case HttpStatusCode.NotModified:
                                sc = Errors.GetStatusCode("HttpStatusCode.NotModified");
                                reason = "Cached copy is up to date, endpoint response wasn't transferred.  HTTP status 304.";
                                ReportConnectivityError(lineNumber, endpoint, sc, reason);
                                break;
                            case HttpStatusCode.PartialContent:
                                sc = Errors.GetStatusCode("HttpStatusCode.PartialContent");
                                reason = "Received partial response.  HTTP status 206.";
                                ReportConnectivityError(lineNumber, endpoint, sc, reason);
                                break;
                            case HttpStatusCode.PaymentRequired:
                                sc = Errors.GetStatusCode("HttpStatusCode.PaymentRequired");
                                reason = "Payment Required.  HTTP status 402.";
                                ReportConnectivityError(lineNumber, endpoint, sc, reason);
                                break;
                            case HttpStatusCode.PreconditionFailed:
                                sc = Errors.GetStatusCode("HttpStatusCode.PreconditionFailed");
                                reason = "Precondition failed.  HTTP status 412.";
                                ReportConnectivityError(lineNumber, endpoint, sc, reason);
                                break;
                            case HttpStatusCode.RedirectKeepVerb:
                                sc = Errors.GetStatusCode("HttpStatusCode.RedirectKeepVerb");
                                reason = "Resource was found but was redirected with the same method.  HTTP status 307.";
                                ReportConnectivityError(lineNumber, endpoint, sc, reason);
                                break;
                            case HttpStatusCode.RedirectMethod:
                                sc = Errors.GetStatusCode("HttpStatusCode.RedirectMethod");
                                reason = "Redirected as a result of the POST.  HTTP status 303.";
                                ReportConnectivityError(lineNumber, endpoint, sc, reason);
                                break;
                            case HttpStatusCode.RequestedRangeNotSatisfiable:
                                sc = Errors.GetStatusCode("HttpStatusCode.RequestedRangeNotSatisfiable");
                                reason = "Range of data requested could not be returned.  HTTP status 416.";
                                ReportConnectivityError(lineNumber, endpoint, sc, reason);
                                break;
                            case HttpStatusCode.RequestTimeout:
                                sc = Errors.GetStatusCode("HttpStatusCode.RequestTimeout");
                                reason = "Request timed out.  HTTP status 408.";
                                ReportConnectivityError(lineNumber, endpoint, sc, reason);
                                break;
                            case HttpStatusCode.ResetContent:
                                sc = Errors.GetStatusCode("HttpStatusCode.ResetContent");
                                reason = "Client should reset the current resource.  HTTP status 205.";
                                ReportConnectivityError(lineNumber, endpoint, sc, reason);
                                break;
                            case HttpStatusCode.SwitchingProtocols:
                                sc = Errors.GetStatusCode("HttpStatusCode.SwitchingProtocols");
                                reason = "Protocol was changed.  HTTP status 101.";
                                ReportConnectivityError(lineNumber, endpoint, sc, reason);
                                break;
                            case HttpStatusCode.UnsupportedMediaType:
                                sc = Errors.GetStatusCode("HttpStatusCode.UnsupportedMediaType");
                                reason = "Response was an unsupported media type.  HTTP status 415.";
                                ReportConnectivityError(lineNumber, endpoint, sc, reason);
                                break;
                            case HttpStatusCode.Unused:
                                sc = Errors.GetStatusCode("HttpStatusCode.Unused");
                                reason = "Unspecified error.  HTTP status 306.";
                                ReportConnectivityError(lineNumber, endpoint, sc, reason);
                                break;
                            case HttpStatusCode.UseProxy:
                                sc = Errors.GetStatusCode("HttpStatusCode.UseProxy");
                                reason = "Request should use proxy server located in the Location header.  HTTP status 305.";
                                ReportConnectivityError(lineNumber, endpoint, sc, reason);
                                break;
                            default:
                                sc = Errors.GetStatusCode("HttpStatusCode.Default");
                                reason = "Connection Failed";
                                ReportConnectivityError(lineNumber, endpoint, sc, reason);
                                break;
                        }

                        response.Close();
                    }
                    catch (WebException ex)
                    {
                        int sc = Errors.GetStatusCode("HttpStatusCode.Default");
                        string reason = ex.Message;
                        ReportConnectivityError(lineNumber, endpoint, sc, reason);
                    }
                }
            }
        }

        private static void ReportConnectivityError(int lineNumber, string endpoint, int sc, string reason)
        {
            Errors.ConnectivityError(lineNumber, endpoint, reason, sc, 1);
            HelperMethods.AddConnection(endpoint, sc, reason, 1, "");
        }

        private static string ExtractEndpoint(string line, string p)
        {
            string l = line.Trim().ToUpper();
            int start = l.LastIndexOf(p.ToUpper()) - 1;
            l = l.Remove(0, start + 1);
            string[] parts = l.Split('"');
            string endpoint = (parts[0].Split('<'))[0].Trim();

            var browseWith = from b in Program.BrowseList where b.Configured.ToUpper() == endpoint.ToUpper() select b;
            if (browseWith != null && browseWith.Count() > 0)
            {
                BrowseObj bo = browseWith.FirstOrDefault();

                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.WriteLine("Testing " + bo.ActualUrl + " in place of " + endpoint + ".");
                Console.ResetColor();

                endpoint = bo.ActualUrl;
            }

            return endpoint;
        }
    }
}
