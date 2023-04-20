# A closer look at Razor Pages and the PageModel

## What we'll be doing in this lab

Now that we have our project and data model set up, we'll be looking more at how the code in our application works with a deeper look at the **PageModel**.

## Understanding the PageModel

Razor Pages are built on top of the MVC framework, and they use a similar controller pattern. However, Razor Pages have a different way of handling the controller logic. Instead of having a separate controller class, the controller logic is handled by a **PageModel** class. The **PageModel** class is a specific class that inherits from the `PageModel` class. The PageModel class is where we put the logic for our Razor Pages.

Let's walk through the code for the **EditModel** in the **Pages/Movies** folder.

### Constructor and Constructor Injection

First, we have our constructor and our injected properties:
    
```csharp
private readonly RazorPagesMovie.Data.RazorPagesMovieContext _context;

public EditModel(RazorPagesMovie.Data.RazorPagesMovieContext context)
{
    _context = context;
}
```

This is a very common pattern throughout ASP.NET Core. We inject the database context into the constructor, and then we use that context to access the database. In the same way, we could inject the `ILogger<EditModel>` into the constructor and use that logger to log messages, or log any of our own custom services.

### PageModel Properties and how PageModel relates to ViewModels

Next, we have our public properties:
    
```csharp
[BindProperty]
public Movie Movie { get; set; } = default!;
```

Here's how that works:
- Any public property on the PageModel class is available to the view.
- Any public property on the PageModel class that is decorated with the `[BindProperty]` attribute is available to the view and can be bound to the view. This means that the view can set the value of the property, and the property can be set from the view.
- You can make many public properties available to the view, without having to create a separate data transfer object (DTO) or ViewModel class.

If you're familiar with ASP.NET MVC, you'll be used to controller actions that return a single model object to the view. This often leads to creating additional ViewModel or data transfer classes just to package up our data, a lot of code in the controller, and exra logic in the view. With Razor Pages, we can use the `PageModel` class to handle the logic, set public properties, and the view can display and edit the properties we intentionally expose directly.

There are security implications to using entity models in your **PageModel** that may necessitate using a **ViewModel** or **DTO**. We'll talk about that in more depth below. While in some cases it may be best to use a ViewModel or DTO object, you do have flexibility with the **PageModel** to easily create as many public properties of any typeas you want to expose them to the view rather than requiring a specific additional class.

### OnGetAsync - Handling GET requests

Razor Pages methods correspond to HTTP verbs. The `OnGetAsync` method corresponds to the HTTP GET verb. The `OnGetAsync` method is called when the page is loaded. The `OnGetAsync` method is where you would put any code that you want to run when the page is loaded. In our case, we're using the `OnGetAsync` method to load the movie movie we're going to be editing. Similarly, there are `OnPostAsync` and `OnDeleteAsync` methods that correspond to the HTTP POST and DELETE verbs, etc.

```csharp
public async Task<IActionResult> OnGetAsync(int? id)
{
    if (id == null || _context.Movie == null)
    {
        return NotFound();
    }

    var movie =  await _context.Movie.FirstOrDefaultAsync(m => m.ID == id);
    if (movie == null)
    {
        return NotFound();
    }
    Movie = movie;
    ViewData["GenreId"] = new SelectList(_context.Genre, "ID", "Name");
    return Page();
}
```

This page expects to be passed an `id` parameter to indicate the movie we're editing, so we first check to see if the `id` parameter is null. If it is, we return a `NotFound` result. If it's not null, we use the `FirstOrDefaultAsync` method to get the movie from the database. If the movie is null, we return a `NotFound` result. If the movie is not null, we set the `Movie` public property to the movie we got from the database, which will allow the edit form to display the current values, and we return the `Page` result.

In general, the common pattern you'll see in `OnGetAsync` looks like this:
1. Check for required parameters.
1. Get required data from the database or service.
1. Perform any specific querying, sorting, or other data manipulation logic.
1. Set public properties that will be used by the view.
1. Return the `Page` result.

In general, **GET** requests should be idempotent, which means that they should not change the state of the server. So in most cases, you'll just use the `OnGetAsync` method to load data and set public properties that will be used by the view. You'll want to use the `OnPostAsync` method to handle form submissions.

### OnPostAsync - Handling POST requests

The `OnPostAsync` method corresponds to the HTTP POST verb. The `OnPostAsync` method is called when the user submits the form. The `OnPostAsync` method is where you would put any code that you want to run when the user submits the form. In our case, we're using the `OnPostAsync` method to validate the submission and save the movie we're editing.

```csharp
// To protect from overposting attacks, enable the specific properties you want to bind to.
// For more details, see https://aka.ms/RazorPagesCRUD.
public async Task<IActionResult> OnPostAsync()
{
    if (!ModelState.IsValid)
    {
        return Page();
    }

    _context.Attach(Movie).State = EntityState.Modified;

    try
    {
        await _context.SaveChangesAsync();
    }
    catch (DbUpdateConcurrencyException)
    {
        if (!MovieExists(Movie.ID))
        {
            return NotFound();
        }
        else
        {
            throw;
        }
    }

    return RedirectToPage("./Index");
}
```

The common pattern for **OnPostAsync** looks like this:
1. Validate the posted data.
1. Attempt to perform the requested action (e.g. save the data to the database).
1. If the action fails, return the `Page` result which will re-display the form with the validation errors.
1. If the action succeeds, return the `RedirectToPage` result which will redirect the user to the specified page. In this case, we're redirecting to the Index page so the user can see the edited movie in the list.

### OverPosting and ViewModels

Note the warning in the comments about [overposting](https://aka.ms/RazorPagesCRUD).  This is where the view posts more data than you expect, and the model is updated with the extra data. For instance, if a user posted a form with a **User** model that also had an `IsAdmin` property, and the user was not an admin, a malicious user could post the form with the `IsAdmin` property set to true programmatically, and the model would be updated with that value. Also, if you're returning a model directly from the controller action, you're exposing the model to the view, and the view can display fields you din't expect or modify the model directly.

For **create** scenarios, you can guard against overposting by explicitly listing bindable properties in the `[Bind]` attribute. For instance, if you only wanted to bind the `Title` and `ReleaseDate` properties, you could use the following:
    
```csharp
public async Task<IActionResult> OnPostAsync([Bind("Title,ReleaseDate")] Movie movie)
{
    if (!ModelState.IsValid)
    {
        return Page();
    }
    //...
```

However, this approach is only appropriate for Create scenarios, as excluded properties are set to null and can overwrite existing values. The general guidance on protection against overposting is that you should consider ViewModels for any scenario where you don't want users to update any fields in the model. In this case, we don't have any fields that we don't want the user to update (especially since these are admin pages that will be behind a login), so we're not going to use a ViewModel yet. It's defintiely a best practice to use in your production applications.

Using the example above, if we only wanted to bind the `Title` and `ReleaseDate` properties, we could create an EditMovieViewModel class that looks like this:

```csharp
public class EditMovieViewModel
{
    public int ID { get; set; }
    public string Title { get; set; }
    public DateTime ReleaseDate { get; set; }}
```

We would expose this property in the PageModel:

```csharp
[BindProperty]
public EditMovieViewModel EditMovieViewModel { get; set; }
```

And we would update our **OnPostAsync** method to use the ViewModel:

```csharp
public async Task<IActionResult> OnPostAsync(int id)
{
    var movieToUpdate = await _context.Movies.FindAsync(id);

    if (movieToUpdate == null)
    {
        return NotFound();
    }

    if (await TryUpdateModelAsync<movie>(
        movieToUpdate,
        "movie",
        m => m.Title, m => m.ReleaseDate))
    {
        await _context.SaveChangesAsync();
        return RedirectToPage("./Index");
    }

    return Page();
}
```

`TryUpdateModel` is a helper method that will update the specified model with the posted data. The first parameter is the model to update, the second parameter is the prefix to use when looking for posted data, and the third parameter is a lambda expression that specifies which properties to update. In this case, we're only updating the `Title` and `ReleaseDate` properties based on the `EditMovieViewModel` properties. 

For **Create** scenarios, you would use the `SetValues` helper method, like this (assuming you had a different `CreateMovieViewModel` class):

```csharp
public async Task<IActionResult> OnPostAsync()
{
    if (!ModelState.IsValid)
    {
        return Page();
    }

    var entry = _context.Add(new Movie());
    entry.CurrentValues.SetValues(CreateMovieViewModel);
    await _context.SaveChangesAsync();
    return RedirectToPage("./Index");
}
```

