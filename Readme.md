How to use
-----------------

### Web API
	
WebApiConfig.cs

	 public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            GlobalConfiguration.Configuration.MessageHandlers.Add(new NLogHandler());
        }


NLogHandler 作用时把WebAPI 请求用NLog.Logger写入，之后与NLog一致，根据NLog.config配置会写到不同输入位置。