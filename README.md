<!-- default badges list -->
![](https://img.shields.io/endpoint?url=https://codecentral.devexpress.com/api/v1/VersionRange/134716005/18.1.3%2B)
[![](https://img.shields.io/badge/Open_in_DevExpress_Support_Center-FF7200?style=flat-square&logo=DevExpress&logoColor=white)](https://supportcenter.devexpress.com/ticket/details/T830587)
[![](https://img.shields.io/badge/📖_How_to_use_DevExpress_Examples-e9f6fc?style=flat-square)](https://docs.devexpress.com/GeneralInformation/403183)
<!-- default badges end -->
# BootstrapUploadControl for ASP.NET Core - How to upload files along with other form data
This example shows how to create a portfolio form, which allows entering a portfolio name and upload attachments. For this, we will use Booststrap-based components for ASP.NET Core: [BootstrapUploadControl](https://demos.devexpress.com/aspnetcore-bootstrap/Editors-UploadControl) and [BootstrapTagBox](https://demos.devexpress.com/aspnetcore-bootstrap/Editors-TagBox).

## Save files to the server RAM or HDD
The example demonstrates two ways to save the uploaded files - into the server memory (using [ASP.NET Core Session](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/app-state?tabs=aspnetcore2x)) or into the Content folder. You can switch between these modes by adding/removing one of these lines in Startup.cs:
```csharp
services.AddSingleton<IFileStorage, PhysicalFileStorage>();
services.AddSingleton<IFileStorage, MemoryFileStorage>();
```
By default, the memory storage is used.

## Steps to implement:
* Create the Portfolio model

```csharp
public class Portfolio {
    [Required]
    public Guid Id { get; set; }
    [Required]
    [Display(Name="Name")]
    public string Name { get; set; }
    [Required]
    [Display(Name = "Files")]
    public string[] FileNames { get; set; }
}
```
* Create two views - one for the [Portfolio form](/UploadControlCore/Views/Home/Index.cshtml), another for [displaying an uploaded portfolio](/UploadControlCore/Views/Home/ViewPortfolio.cshtml)
* Add editors to the form

```csharp 
@(Html.DevExpress().BootstrapTextBoxFor(m => m.Name))
@(Html.DevExpress().BootstrapTagBoxFor(m => m.FileNames))
@(Html.DevExpress().BootstrapUploadControl("UploadControl") ... )
```

* Specify the upload route for the Upload Control:

```csharp 
.Routes(routes => routes.MapRoute(r => r
    .RouteValues(new {
        Controller = "Home",
        Action = "UploadFile",
        Id = Model.Id
    })
```

* In the specified action, implement saving logic:

```csharp
public IActionResult UploadFile(BootstrapUploadedFilesCollection files, Guid id) {
    files.ForEach(file => {
        using (var fileStream = file.OpenReadStream()) {
            byte[] data = new byte[fileStream.Length];
            fileStream.Read(data, 0, data.Length);.Routes(routes => routes
            Storage.SaveFile(id, data, file.FileName);
        }   
        file.CustomData["name"] = file.FileName; 
    });
    return Ok(files);)
}
```

* In the form view, specify the FileUploadComplete event handler

```csharp  
.ClientSideEvents(events => events.FileUploadComplete("onFileUploadComplete"))
``` 
```javascript
function onFileUploadComplete(s, e) {
    FileNames.AddTag(e.customData["name"]);
}
```
This will display uploaded files in the Tag Box.

* In the submit action, check whether the portfolio is valid and display the second view:

```csharp 
public IActionResult CreatePortfolio(Portfolio portfolio) {
    if (ModelState.IsValid)
        return View("ViewPortfolio", portfolio);

    return View("Index", portfolio);
}
```

* In the ViewPortfolio, use the [BootstrapListBox](https://demos.devexpress.com/aspnetcore-bootstrap/Editors-ListBox) to display uploaded files:

```csharp
@(Html.DevExpress().BootstrapListBoxFor(m => m.FileNames)
    .CaptionSettings(captionSettings => captionSettings.Hidden(true))
    .Bind(Model.FileNames)
    .ItemTemplate(t => @<text>
    <div class="row">
        <a class="col-12" href="@Url.Action("ViewImage", "Home", new { PortfolioId = Model.Id, Name = t.Item.Text })">@t.Item.Text</a>
    </div>
    </text>)
)
```

* To view an image using a link in the List Box, implement the following controller action:

```csharp
public IActionResult ViewImage(Guid portfolioId, string name) {
    byte[] fileData = Storage.GetFilesByPortfolioId(portfolioId).Where(f => f.FileName == name).FirstOrDefault().Bytes;
    return File(fileData, "image/jpg");
}
```
