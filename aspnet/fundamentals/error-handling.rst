Error Handling
==============
By `Steve Smith`_

When errors occur in your ASP.NET app, you can handle them in a variety of ways, as described in this article.

.. contents:: Sections
	:local:
	:depth: 1
	
`View sample files <https://github.com/aspnet/Docs/tree/master/aspnet/fundamentals/error-handling/sample>`_

Configuring an error handling page
----------------------------------

In ASP.NET, you configure the pipeline for each request in the ``Startup`` class's ``Configure()`` method (learn more about :doc:`configuration`). You can add a simple error page, meant only for use during development, very easily. All that's required is to add a dependency on ``Microsoft.AspNet.Diagnostics`` to the project and then add one line to ``Configure()`` in ``Startup.cs``:

.. literalinclude:: error-handling/sample/src/ErrorHandlingSample/Startup.cs
	:language: csharp
	:lines: 18-26
	:dedent: 8
	:emphasize-lines: 6,8

The above code, which is built from the ASP.NET Empty Visual Studio template, includes a check to ensure the environment is development before adding the call to ``UseDeveloperExceptionPage``. This is a good practice, since you typically do not want to share detailed exception information about your application publicly while it is in production. :doc:`Learn more about configuring environments <environments>`.

The sample application includes a simple mechanism for creating an exception:

.. literalinclude:: error-handling/sample/src/ErrorHandlingSample/Startup.cs
	:language: csharp
	:lines: 35-48
	:dedent: 12
	:emphasize-lines: 3-6

If a request includes a non-empty querystring parameter for the variable ``throw`` (e.g. a path of ``/?throw=true``), an exception will be thrown. If the environment is set to ``Development``, the developer exception page is displayed:

.. image:: error-handling/_static/developer-exception-page.png

When not in development, it's a good idea to configure friendly pages for common HTTP status codes. You can use :ref:`status code pages <status-code-pages>` for this purpose.

Using the Developer Exception Page
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

The developer exception page displays useful diagnostics information when an unhandled exception occurs within the web processing pipeline. The page includes several tabs with information about the exception that was triggered and the request that was made. The first tab includes a stack trace:

.. image:: error-handling/_static/developer-exception-page.png

The next tab shows the contents of the Querystring collection, if any:

.. image:: error-handling/_static/developer-exception-page-query.png

In this case, you can see the value of the ``throw`` parameter that was passed to this request. This request didn't have any cookies, but if it did, they would appear on the Cookies tab. You can see the headers that were passed in the last tab:

.. image:: error-handling/_static/developer-exception-page-headers.png

HTTP 500 errors on Azure
------------------------

If your app throws an exception before the ``Configure`` method in *Startup.cs* completes, the developer exception page won't be configured. The app deployed to Azure (or another production server) will return an HTTP 500 error with no message details.

The publish wizard in Visual Studio 2015 creates a *web.config* file if you don't have one. If you have a *web.config* file in the *wwwroot* folder, the deploy process inserts the markup into the *web.config* file it generates. 

To get detailed error messages on Azure, add the following *web.config* file to the *wwwroot* folder.

.. warning:: Security warning: Enabling detailed error message can leak critical information from your app. You should never enable detailed error messages on a production app.

.. code-block:: html

	<configuration>
	   <system.web>
		  <customErrors mode="Off"/>
	   </system.web>
	</configuration>

Exception Filters
-----------------

Exception filters can be configured globally or on a per-controller or per-action basis in an :doc:`MVC </mvc/index>` app. These filters handle any unhandled exception that occurs during the execution of a controller action or another filter, and are not called otherwise. Exception filters are detailed in :doc:`filters </mvc/controllers/filters>`.

.. tip:: Exception filters are good for trapping exceptions that occur within MVC actions, but they're not as flexible as error handling middleware. Prefer middleware for the general case, and use filters only where you need to do error handling *differently* based on which MVC action was chosen.

.. _status-code-pages:

Configuring Status Code Pages
-----------------------------

By default, your ASP.NET app will not provide a rich status code page for HTTP status codes such as 500 (error) or 404 (not found). You can configure the ``StatusCodePagesMiddleware`` adding this line to the ``Configure`` method:

.. code-block:: c#

	app.UseStatusCodePages();

By default, this middleware adds very simple, text-only handlers for common status codes. For example, the following is the result of a 404 Not Found status code:

.. image:: error-handling/_static/default-404-status-code.png

The middleware supports several different extension methods. You can pass it a custom lamba expression:

.. code-block:: c#

	app.UseStatusCodePages(context => 
		context.HttpContext.Response.SendAsync("Handler, status code: " +
		context.HttpContext.Response.StatusCode, "text/plain"));

Alternately, you can simply pass it a content type and a format string:

.. code-block:: c#

	app.UseStatusCodePages("text/plain", "Response, status code: {0}");

The middleware can handle redirects (with either relative or absolute URL paths), passing the status code as part of the URL:

.. code-block:: c#

	app.UseStatusCodePagesWithRedirects("~/errors/{0}");

In the above case, the client browser will see a ``302 / Found`` status and will redirect to the URL provided.

Alternately, the middleware can re-execute the request from a new path format string:

.. code-block:: c#

	app.UseStatusCodePagesWithReExecute("/errors/{0}");

The ``UseStatusCodePagesWithReExecute`` method will still return the original status code to the browser, but will also execute the handler given at the path specified.

If you need to disable status code pages for certain requests, you can do so using the following code:

.. code-block:: c#

	var statusCodePagesFeature = context.Features.Get<IStatusCodePagesFeature>();
	if (statusCodePagesFeature != null)
	{
		statusCodePagesFeature.Enabled = false;
	}

Limitations of Error Handling During Client-Server Interaction
--------------------------------------------------------------

ASP.NET apps have certain limitations to their error handling capabilities, because of the nature of disconnected HTTP requests and responses. Keep these in mind as you design your app's error handling capabilities.

#. Once the headers for a response have been sent, you cannot change the response's status code.
#. If the client disconnects mid-response, you cannot send them the rest of the content of that response.
#. There is always the possibility of an error occuring one layer below your error handling layer.
#. Don't forget, error handling pages can have errors, too. It's often a good idea for production error pages to consist of purely static content.

Following the above recommendations will help ensure your app remains responsive and is able to gracefully handle errors that may occur.

Server Error Handling
---------------------

In addition to ASP.NET, the server hosting your app will perform some error handling. In the case of IIS, this is described `here <https://technet.microsoft.com/en-us/library/cc731570(v=ws.10).aspx>`. For Kestrel, its built-in error handling behavior may be found `here <https://github.com/aspnet/KestrelHttpServer/blob/dev/src/Microsoft.AspNetCore.Server.Kestrel/Http/Frame.cs#L565>`. Requests that are not handled by your app will be handled by the server, and any error that occurs will be handled by the server's error handling. Any custom error pages or error handling middleware or filters you have configured for your app will not affect this behavior.

Startup Error Handling
----------------------

Ex. A startup exception could prevent SSL from being enabled (see aspnet/KestrelHttpServer#369).

Opt-out using UseCaptureStartupErrors.
(link to this from Startup)

(below are MVC specific)

Handling Model State Errors

Model validation occurs prior to each controller action being invoked, and it is the action method’s responsibility to inspect ModelState.IsValid and react appropriately. In many cases, the appropriate reaction is to return some kind of error response, ideally detailing the reason why model validation failed.

Some apps will choose to follow a standard convention for dealing with model validation errors, in which case a filter may be an appropriate place to implement such a policy (see ValidateModelAttribute). You should test how your actions behave with valid and invalid model states (learn more about testing controller logic).

Using SerializableError

Validation errors need to be consumable by web API client applications. SerializableError wraps aModelStateDictionary for serialization to XML or Json (or other wire formats) for consumption by client-side code.