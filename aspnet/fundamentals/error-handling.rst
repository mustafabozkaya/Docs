Error Handling
==============
By `Steve Smith`_

Views frequently share visual and programmatic elements. In this article, you'll learn how to use common layouts, share directives, and run common code before rendering views in your ASP.NET app.

.. contents:: Sections
	:local:
	:depth: 1
	
`View sample files <https://github.com/aspnet/Docs/tree/master/aspnet/mvc/views/layout/sample>`_

.. _configure-error-page:

Configuring an error handling page
----------------------------------

In ASP.NET 5, you configure the pipeline for each request in the ``Startup`` class's ``Configure()`` method (learn more about :doc:`configuration`). You can add a simple error page, meant only for use during development, very easily. All that's required is to add a dependency on ``Microsoft.AspNet.Diagnostics`` to the project (and a ``using`` statement to ``Startup.cs``), and then add one line to ``Configure()`` in ``Startup.cs``:

.. _diag-startup:

.. literalinclude:: diagnostics/sample/src/DiagDemo/Startup.cs
	:language: csharp
	:linenos:
	:emphasize-lines: 2,21

The above code, which is built from the ASP.NET 5 Empty template, includes a simple mechanism for creating an exception on line 36. If a request includes a non-empty querystring parameter for the variable ``throw`` (e.g. a path of ``/?throw=true``), an exception will be thrown. Line 21 makes the call to ``UseDeveloperExceptionPage()`` to enable the error page middleware. 

Notice that the call to ``UseDeveloperExceptionPage()`` is wrapped inside an ``if`` condition that checks the current ``EnvironmentName``. This is a good practice, since you typically do not want to share detailed diagnostic information about your application publicly once it is in production. This check uses the ``ASPNET_ENV`` environment variable. If you are using Visual Studio 2015, you can customize the environment variables used when the application runs in the web application project's properties, under the Debug tab, as shown here:

.. image:: diagnostics/_static/project-properties-env-vars.png
	
Setting the ``ASPNET_ENV`` variable to anything other than Development (e.g. Production) will cause the application not to call ``UseDeveloperExceptionPage()`` and only a HTTP 500 response code with no message details will be sent back to the browser, unless an exception handler is configured such as ``UseExceptionHandler()``.

We will cover the features provided by the error page in the next section (ensure ``ASPNET_ENV`` is reset to Development if you are following along).

Using the error page during development
---------------------------------------

The default error page will display some useful diagnostics information when an unhandled exception occurs within the web processing pipeline. The error page includes several tabs with information about the exception that was triggered and the request that was made. The first tab shows the stack trace:

.. image:: diagnostics/_static/errorpage-stack.png

The next tab shows the contents of the Querystring collection, if any:

.. image:: diagnostics/_static/errorpage-query.png

In this case, you can see the value of the ``throw`` parameter that was passed to this request. This request didn't have any cookies, but if it did, they would appear on the Cookies tab. You can see the headers that were passed, here:

.. image:: diagnostics/_static/errorpage-headers.png

.. note:: In the current pre-release build, the Cookies section of ErrorPage is not yet enabled. `View ErrorPage Source <https://github.com/aspnet/Diagnostics/blob/dev/src/Microsoft.AspNet.Diagnostics/Views/ErrorPage.cshtml>`_.

HTTP 500 errors on Azure
-------------------------

If your app throws an exception before the ``Configure`` method in *Startup.cs* completes, the error page won't be configured. The app deployed to Azure (or another production server) will return an HTTP 500 error with no message details. ASP.NET 5 uses a new configuration model that is not based on *web.config*, and when you create a new web app with Visual Studio 2015, the project no longer contains a *web.config* file. (See `Understanding ASP.NET 5 Web Apps <http://docs.asp.net/en/latest/conceptual-overview/understanding-aspnet5-apps.html>`_.)

The publish wizard in Visual Studio 2015 creates a *web.config* file if you don't have one. If you have a *web.config* file in the *wwwroot* folder, deploy inserts the markup into the the *web.config* file it generates. 

To get detailed error messages on Azure, add the following *web.config* file to the *wwwroot* folder.

.. note:: Security warning: Enabling detailed error message can leak critical information from your app. You should never enable detailed error messages on a production app.

.. code-block:: html

	<configuration>
	   <system.web>
		  <customErrors mode="Off"/>
	   </system.web>
	</configuration>





Exception Filters
-----------------

Exception filters can be configured globally or on a per-controller or per-action basis. These filters can react to any unhandled exception that occurs during the execution of a controller action or another filter. Exception filters are detailed in filters.

(here is what is currently in filters)

*Exception Filters* implement either the ``IExceptionFilter`` or ``IAsyncExceptionFilter`` interface.

Exception filters handle unhandled exceptions. They are only called when an exception occurs later in the pipeline. They can provide a single location to implement common error handling policies within an app. The framework provides an abstract `ExceptionFilterAttribute <https://docs.asp.net/projects/api/en/latest/autoapi/Microsoft/AspNet/Mvc/Filters/ExceptionFilterAttribute/index.html>`_ that you should be able to subclass for your needs. Exception filters are good for trapping exceptions that occur within MVC actions, but they're not as flexible as error handling middleware. Prefer middleware for the general case, and use filters only where you need to do error handling *differently* based on which MVC action was chosen.

.. tip:: One example where you might need a different form of error handling for different actions would be in an app that exposes both API endpoints and actions that return views/HTML. The API endpoints could return error information as JSON, while the view-based actions could return an error page as HTML.

Exception filters do not have two events (for before and after) - they only implement ``OnException`` (or ``OnExceptionAsync``). The ``ExceptionContext`` provided in the ``OnException`` parameter includes the ``Exception`` that occurred. If you set ``context.Exception`` to null, the effect is that you've handled the exception, so the request will proceed as if it hadn't occurred (generally returning a 200 OK status). The following filter uses a custom developer error view to display details about exceptions that occur when the application is in development:

.. literalinclude:: filters/sample/src/FiltersSample/Filters/CustomExceptionFilterAttribute.cs
  :language: c#
  :emphasize-lines: 33-34

Configuring an status code pages
--------------------------------

You can configure the StatusCodePagesMiddleware by calling:

app.UseStatusCodePages();

Limitations of Error Handling During Client-Server Interaction
--------------------------------------------------------------

If the headers are already sent you can't send a different status code
If the client disconnects you can't send the rest of the content
An error can always happen one layer below your error handling layer
Error pages can error too - probably should be static content only

Server Error Handling
---------------------

In addition to ASP.NET, the server hosting your app will perform some error handling. In the case of IIS, this is described (here). For Kestrel, its built-in error handling behavior is described (here). Requests that are handled directly by the server, rather than by ASP.NET, will be handled by the server's error handling. Any custom error pages or error handling middleware or filters you have configured for your app will not affect this behavior.

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