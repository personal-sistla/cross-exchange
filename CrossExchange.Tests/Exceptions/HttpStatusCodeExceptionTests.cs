using System;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace CrossExchange.Tests.Exceptions
{
    public class HttpStatusCodeExceptionTests
    {
        [Test]
        public void CheckStatusCodeFlags()
        {
            var e1 = new HttpStatusCodeException(10);
            Assert.AreEqual(10, e1.StatusCode);

            var e2 = new HttpStatusCodeException(101, new Exception("Inner Ex"));
            Assert.AreEqual(101, e2.StatusCode);
            Assert.AreEqual("System.Exception: Inner Ex", e2.Message);

            var e3 = new HttpStatusCodeException(20, "exc1");
            Assert.AreEqual(20, e3.StatusCode);
            Assert.AreEqual("exc1", e3.Message);
            Assert.AreEqual("text/plain", e3.ContentType);

            var jsonErr = new JObject();
            jsonErr["m"] = "error";
            var e4 = new HttpStatusCodeException(201, jsonErr);
            Assert.AreEqual(201, e4.StatusCode);
            Assert.AreEqual("{\r\n  \"m\": \"error\"\r\n}", e4.Message);
            Assert.AreEqual("application/json", e4.ContentType);
        }
    }
}
