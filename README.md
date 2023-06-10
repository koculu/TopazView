# TopazView

TopazView is a lightweight view engine that utilizes the Topaz JavaScript Engine. It serves as an alternative to the Razor View Engine, offering several key features and benefits.

[![Downloads](https://img.shields.io/nuget/dt/TopazView)](https://www.nuget.org/packages/TopazView/)

## Key Features:

* **Faster startup:** TopazView is designed to provide quicker startup times compared to other view engines.
* **Faster dynamic rendering:** With TopazView, dynamic rendering is optimized for improved performance.
* **JavaScript syntax familiarity:** TopazView employs JavaScript syntax, making it more accessible and familiar to web developers.
* **Simple learning curve:** The template syntax used in TopazView closely resembles Razor, ensuring a smooth learning curve for developers.
* **Support for page models, layouts, sections, helper functions:** TopazView provides comprehensive support for page models, layouts, sections, and helper functions, enabling developers to create dynamic and modular views.
* **Complete .NET API support:** TopazView leverages the power of Topaz JavaScript engine to provide comprehensive access to the entire .NET API, giving developers extensive flexibility in their web development.

## How to use TopazView?

The following sample shows the most basic setup of the TopazView.

```C#
using Tenray.TopazView;

var path = "web/views";
var viewEngine = new ViewEngineFactory()
    .SetContentProvider(new FileSystemContentProvider(path))
    .CreateViewEngine();

var contentWatcher = new FileSystemContentWatcher();
contentWatcher.StartWatcher(path, viewEngine);
```

The following is a sample of view rendering.

```C#
using Tenray.TopazView;

var context = viewEngine.CreateViewRenderContext();
context.Model = new { Title = "My Awesome TopazView Template" }.ToJsObject();
var html = await ViewEngine
    // provide an existing text file's relative path with any extension.
    .GetOrCreateView("/home/index.view")
    .GetCompiledView()
    .RenderViewToString(context);

```

The following is a sample view template file (index.view).
```cshtml
@Layout="../layouts/layout.view"

<div>@model.Title</div>

@{
    for(const i of [1,2,3]) {
        page.raw(i + ' <br>')
    }
}

@if(@model.Title) {
    <div>model has a title.</div>
}
else {
    <div>model has no title.</div>
}
```


## TODO list:

To further enhance TopazView, the following tasks are on the agenda:

* **Write documentation:** Thorough documentation is essential for enabling developers to understand and utilize TopazView effectively.
* **Provide samples:** Offering a collection of sample applications and code snippets will assist users in grasping the practical usage of TopazView.
* **Create developer-friendly exception page:** Building an exception page specifically tailored to developers will facilitate error debugging and enhance the overall development experience.

Feel free to contribute to the project by tackling any of these tasks or suggesting additional improvements. Your involvement is greatly appreciated!
