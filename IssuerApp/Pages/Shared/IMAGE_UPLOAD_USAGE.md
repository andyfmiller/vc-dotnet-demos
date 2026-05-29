# Image Upload Modal - Reusable Component

This document describes how to use the reusable image upload modal component across different pages.

## Components

1. **`_ImageUploadModalPartial.cshtml`** - Partial view for the modal UI
2. **`image-upload-modal.js`** - Client-side JavaScript for modal behavior
3. **`IImageUploadPageModel`** - Interface for page models
4. **`ImageUploadHelper`** - Static helper methods for processing uploads

## Usage

### 1. In the Page Model (.cshtml.cs)

Implement the `IImageUploadPageModel` interface and add the upload handler:

```csharp
public class EditModel : IssuerAppPageModel, IImageUploadPageModel
{
    [BindProperty]
    [Display(Name = "Image", Description = "An image representing the entity.")]
    public string ImageId { get; set; }

    [BindProperty, Display(Name = "Image File")]
    [FormFileExtensions(Extensions = "png,jpg,jpeg,gif,svg")]
    public IFormFile ImageFile { get; set; }

    public async Task<IActionResult> OnPostUploadImage()
    {
        // Step 1: Define entity-specific validation clearing logic
        // This is REQUIRED because each entity has different validation requirements
        void ClearValidationErrors(EditModel model, ModelStateDictionary modelState)
        {
            // Initialize required collections and clear validation errors for fields not submitted by the modal
            // Copy the validation clearing logic from your OnPostAsync() method

            // Example for Organization entity:
            if (model.Organization?.Profile != null && (model.Organization.Profile.Type == null || !model.Organization.Profile.Type.Any()))
            {
                model.Organization.Profile.Type = new List<string> { "Profile" };
            }
            modelState.Remove($"{nameof(Organization)}.{nameof(Organization.Profile)}.{nameof(Organization.Profile.Type)}");

            // Add similar logic for other required fields...
        }

        // Step 2: Reload entity from database since it wasn't fully submitted
        if (!ModelState.IsValid)
        {
            YourEntity = await Context.YourEntities
                .Include(x => x.Image)  // Adjust based on your structure
                .FirstOrDefaultAsync(x => x.YourEntityKey == YourEntity.YourEntityKey);

            if (YourEntity != null)
            {
                ImageId = YourEntity.Image?.Id;  // Adjust based on your structure
            }

            return Page();
        }

        try
        {
            // Step 3: Reload entity from database
            YourEntity = await Context.YourEntities
                .Include(x => x.Image)  // Adjust based on your structure
                .FirstOrDefaultAsync(x => x.YourEntityKey == YourEntity.YourEntityKey);

            if (YourEntity == null)
            {
                ModelState.AddModelError(string.Empty, "Cannot find the entity.");
                return Page();
            }

            // Step 4: Process the uploaded image with validation clearing callback
            var dataUri = await ImageUploadHelper.ProcessImageUpload(
                this, ModelState, Logger, ClearValidationErrors);

            if (!ModelState.IsValid)
            {
                ImageId = YourEntity.Image?.Id;
                return Page();
            }

            // Step 5: Update the image based on your entity structure
            // For entities with direct Image property (Achievement, AchievementCredential):
            YourEntity.Image = ImageUploadHelper.CreateOrUpdateImage(YourEntity.Image, dataUri);

            // OR for entities with nested Image (Organization.Profile.Image):
            // YourEntity.Profile.Image = ImageUploadHelper.CreateOrUpdateImage(
            //     YourEntity.Profile.Image, dataUri);

            ImageId = dataUri;

            // Step 6: Update ImageId in ModelState for form display
            var imageValue = ModelState.FirstOrDefault(x => x.Key == nameof(ImageId)).Value;
            if (imageValue != null)
            {
                imageValue.RawValue = ImageId;
            }

            // Step 7: Signal successful upload for modal auto-close
            TempData["ImageUploaded"] = "true";
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error trying to upload image");
            ModelState.AddModelError(nameof(ImageId), e.Message);
        }

        return Page();
    }
}
```

### 2. In the Razor Page (.cshtml)

Add the image upload modal partial and initialize the JavaScript:

```razor
@* In the form where you want the upload button *@
<div class="input-group input-group-sm">
    <label asp-for="ImageId" class="input-group-text"></label>
    @if (!string.IsNullOrEmpty(Model.ImageId))
    {
        <div class="input-group-text">
            @if (Model.ImageId.StartsWith("data:"))
            {
                <a href="#" onclick="exportToView('@Model.ImageId')">Preview</a>
            }
            else
            {
                <a href="@Model.ImageId" target="_blank">Preview</a>
            }
        </div>
    }
    <input asp-for="ImageId" class="form-control" placeholder="Enter URL or upload file" />
    <button type="button" class="btn btn-dark" data-bs-toggle="modal" data-bs-target="#upload">Upload Image</button>
    <div class="input-group-text">
        <description asp-for="ImageId">
            An image representing the entity.
        </description>
    </div>
</div>
<span asp-validation-for="ImageId" class="text-danger"></span>

@* Include the modal partial *@
@{
    ViewData["ModalId"] = "upload";
    ViewData["EntityKeyName"] = "YourEntity.YourEntityKey";  // Adjust based on your entity
    ViewData["EntityKey"] = Model.YourEntity.YourEntityKey;
    ViewData["ModalTitle"] = "Upload Image File";
    ViewData["FileLabel"] = "Choose an image that represents the entity";
}
<partial name="_ImageUploadModalPartial" />

@section Scripts {
    @* Include the shared script *@
    <script src="~/js/image-upload-modal.js"></script>

    <script>
        // Initialize the image upload modal
        ImageUploadModal.init({
            modalId: 'upload',
            fileInputId: 'ImageFile',
            uploadButtonId: 'upload-btn'
        });

        // Close modal after successful upload
        if ('@TempData["ImageUploaded"]' === 'true') {
            ImageUploadModal.closeAndReset('upload', 'ImageFile');
        }
    </script>
}
```

## Entity-Specific Examples

### Organization (Nested Image: Organization.Profile.Image)

**Complete Working Example:**

```csharp
public async Task<IActionResult> OnPostUploadImage()
{
    void ClearValidationErrors(EditModel model, ModelStateDictionary modelState)
    {
        // Ensure Profile.Type collection is initialized
        if (model.Organization?.Profile != null && 
            (model.Organization.Profile.Type == null || !model.Organization.Profile.Type.Any()))
        {
            model.Organization.Profile.Type = new List<string> { "Profile" };
        }
        modelState.Remove($"{nameof(Organization)}.{nameof(Organization.Profile)}.{nameof(Organization.Profile.Type)}");

        // Ensure Address.Type collection is initialized
        if (model.Organization?.Profile?.Address != null && 
            (model.Organization.Profile.Address.Type == null || !model.Organization.Profile.Address.Type.Any()))
        {
            model.Organization.Profile.Address.Type = new List<string> { "Address" };
        }
        modelState.Remove($"{nameof(Organization)}.{nameof(Organization.Profile)}.{nameof(Organization.Profile.Address)}.{nameof(Organization.Profile.Address.Type)}");

        // Handle Image validation
        if (string.IsNullOrWhiteSpace(model.ImageId))
        {
            modelState.Remove($"{nameof(Organization)}.{nameof(Organization.Profile)}.{nameof(Organization.Profile.Image)}.{nameof(Organization.Profile.Image.Id)}");
            modelState.Remove($"{nameof(Organization)}.{nameof(Organization.Profile)}.{nameof(Organization.Profile.Image)}.{nameof(Organization.Profile.Image.Type)}");
            modelState.Remove($"{nameof(Organization)}.{nameof(Organization.Profile)}.{nameof(Organization.Profile.Image)}.{nameof(Organization.Profile.Image.ImageKey)}");
            if (model.Organization?.Profile != null)
            {
                model.Organization.Profile.Image = null;
            }
        }
        else
        {
            if (model.Organization?.Profile?.Image == null && model.Organization?.Profile != null)
            {
                model.Organization.Profile.Image = new Image { Type = "Image", Id = model.ImageId };
            }
            else if (model.Organization?.Profile?.Image != null)
            {
                model.Organization.Profile.Image.Id = model.ImageId;
                if (string.IsNullOrEmpty(model.Organization.Profile.Image.Type))
                {
                    model.Organization.Profile.Image.Type = "Image";
                }
            }

            modelState.Remove($"{nameof(Organization)}.{nameof(Organization.Profile)}.{nameof(Organization.Profile.Image)}.{nameof(Organization.Profile.Image.Id)}");
            modelState.Remove($"{nameof(Organization)}.{nameof(Organization.Profile)}.{nameof(Organization.Profile.Image)}.{nameof(Organization.Profile.Image.Type)}");
            modelState.Remove($"{nameof(Organization)}.{nameof(Organization.Profile)}.{nameof(Organization.Profile.Image)}.{nameof(Organization.Profile.Image.ImageKey)}");
        }

        // Clear AddressKey validation
        modelState.Remove($"{nameof(Organization)}.{nameof(Organization.Profile)}.{nameof(Organization.Profile.Address)}.{nameof(Organization.Profile.Address.AddressKey)}");
    }

    if (!ModelState.IsValid)
    {
        Organization = await Context.Organizations
            .Include(x => x.Profile).ThenInclude(x => x.Image)
            .Include(x => x.Profile).ThenInclude(x => x.Address)
            .FirstOrDefaultAsync(x => x.OrganizationKey == Organization.OrganizationKey);

        if (Organization != null)
        {
            ImageId = Organization.Profile?.Image?.Id;
        }

        return Page();
    }

    try
    {
        Organization = await Context.Organizations
            .Include(x => x.Profile).ThenInclude(x => x.Image)
            .Include(x => x.Profile).ThenInclude(x => x.Address)
            .FirstOrDefaultAsync(x => x.OrganizationKey == Organization.OrganizationKey);

        if (Organization == null)
        {
            ModelState.AddModelError(string.Empty, "Cannot find the organization.");
            return Page();
        }

        var dataUri = await ImageUploadHelper.ProcessImageUpload(
            this, ModelState, Logger, ClearValidationErrors);

        if (!ModelState.IsValid)
        {
            ImageId = Organization.Profile?.Image?.Id;
            return Page();
        }

        Organization.Profile.Image = ImageUploadHelper.CreateOrUpdateImage(
            Organization.Profile.Image, dataUri);
        ImageId = dataUri;

        var imageValue = ModelState.FirstOrDefault(x => x.Key == nameof(ImageId)).Value;
        if (imageValue != null)
        {
            imageValue.RawValue = ImageId;
        }

        TempData["ImageUploaded"] = "true";
    }
    catch (Exception e)
    {
        Logger.LogError(e, "Error trying to upload image");
        ModelState.AddModelError(nameof(ImageId), e.Message);
    }

    return Page();
}
```

### Achievement (Direct Image: Achievement.Image)

**Simplified Example** (Achievement typically has fewer validation requirements):

```csharp
public async Task<IActionResult> OnPostUploadImage()
{
    void ClearValidationErrors(EditModel model, ModelStateDictionary modelState)
    {
        // Clear validation errors for fields not submitted by modal
        // Achievement entity typically has simpler validation requirements
        // Add any entity-specific clearing logic here
    }

    if (!ModelState.IsValid)
    {
        Achievement = await Context.Achievements
            .Include(x => x.Image)
            .FirstOrDefaultAsync(x => x.AchievementKey == Achievement.AchievementKey);

        if (Achievement != null)
        {
            ImageId = Achievement.Image?.Id;
        }

        return Page();
    }

    try
    {
        Achievement = await Context.Achievements
            .Include(x => x.Image)
            .FirstOrDefaultAsync(x => x.AchievementKey == Achievement.AchievementKey);

        if (Achievement == null)
        {
            ModelState.AddModelError(string.Empty, "Cannot find the achievement.");
            return Page();
        }

        var dataUri = await ImageUploadHelper.ProcessImageUpload(
            this, ModelState, Logger, ClearValidationErrors);

        if (!ModelState.IsValid)
        {
            ImageId = Achievement.Image?.Id;
            return Page();
        }

        Achievement.Image = ImageUploadHelper.CreateOrUpdateImage(Achievement.Image, dataUri);
        ImageId = dataUri;

        var imageValue = ModelState.FirstOrDefault(x => x.Key == nameof(ImageId)).Value;
        if (imageValue != null)
        {
            imageValue.RawValue = ImageId;
        }

        TempData["ImageUploaded"] = "true";
    }
    catch (Exception e)
    {
        Logger.LogError(e, "Error trying to upload image");
        ModelState.AddModelError(nameof(ImageId), e.Message);
    }

    return Page();
}
```

### AchievementCredential (Direct Image: AchievementCredential.Image)

Similar to Achievement - adjust entity name and key property.

## Key Points

1. **Validation clearing is entity-specific**: Each entity has different validation requirements. Copy the validation clearing logic from your `OnPostAsync()` method.

2. **The helper simplifies, but doesn't eliminate complexity**: The `ImageUploadHelper.ProcessImageUpload()` method handles the common file processing logic, but you still need to provide entity-specific validation clearing.

3. **Reload entity after validation fails**: Always reload the full entity from the database when returning `Page()` after validation errors.

4. **Update ModelState after successful upload**: Use the pattern shown to update `imageValue.RawValue` so the form displays the new image URL.

## Benefits

- ✅ **Eliminates duplicate file processing code** - The base64 encoding and error handling is centralized
- ✅ **Consistent modal UI** - Single source of truth for the modal markup
- ✅ **Flexible** - Callback pattern allows entity-specific validation logic
- ✅ **Type-safe** - Generic constraints ensure PageModel compatibility
- ✅ **Well-documented** - Real-world examples from working code
