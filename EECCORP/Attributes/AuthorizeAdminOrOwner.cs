//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;

//namespace EECCORP.Attributes
//{
//    //[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
//    //public class AuthorizeAdminOrOwnerAttribute : AuthorizeAttribute
//    //{
//    //    public AuthorizeAdminOrOwnerAttribute(string id)
//    //    {

//    //    }
//    //}

//    //public class AuthorizeAdminOrOwner : AuthorizeAttribute
//    //{
//    //    public string UserId { get; set; }

//    //    protected override bool AuthorizeCore(HttpContextBase httpContext)
//    //    {
//    //        var isAuthorized = base.AuthorizeCore(httpContext);
//    //        var id = 0;

//    //        if (!isAuthorized)
//    //        {
//    //            return false;
//    //        }

//    //        if (httpContext.Pa)
            

//    //        return httpContext.User.IsInRole("Admin") || httpContext.User.Identity.Name == UserId;
            
//    //    }
//    //}


//    public class AuthorizeAdminOrOwnerAttribue : AuthorizeAttribute
//    {
//        public string UserId { get; set; }

        

//        //public override void OnAuthorization(AuthorizationContext filterContext)
//        //{
//        //    base.OnAuthorization(filterContext);

//        //    //filterContext.
//        //}

//        //public override bool onAuthorizeCore
//    }
//}