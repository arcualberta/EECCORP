using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EECCORP.Models;

namespace EECCORP_Tests
{
    [TestClass]
    public class UserTests
    {
        [TestMethod]
        public void TestUserHasRegistration()
        {
            Registration registration = new Registration();
            ApplicationUser user = new ApplicationUser();
            user.Registrations.Add(registration);
            Assert.IsTrue(user.Registrations.Contains(registration));
        }
    }
}
