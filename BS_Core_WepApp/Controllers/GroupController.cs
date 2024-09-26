using BS_Core_WepApp.Key;
using BS_Core_WepApp.Services;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System.IO;
using System;
using Microsoft.AspNetCore.Http;
using BS_Core_WepApp.Models;
using Firebase.Storage;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using X.PagedList;

namespace BS_Core_WepApp.Controllers
{
    public class GroupController : Controller
    {
        private IHostEnvironment _env;
        private IJavaScriptService _javascriptService;
        FirestoreDb firestoreDb;
        public GroupController(IHostEnvironment env, IJavaScriptService javaScriptService)
        {
            _javascriptService = javaScriptService;
            _env = env;
            string keyPath = Path.Combine(_env.ContentRootPath, "Key\\newjoybayiapp-bb25d-firebase-adminsdk-gim0h-8d4f3dc2cb.json");
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", keyPath);
            firestoreDb = FirestoreDb.Create(cls_keys.projectId);
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("bt_userToken") != null)
            {

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Index(GroupModel groupModel, int page = 1)
        {
            if (HttpContext.Session.GetString("bt_userToken") != null)
            {
                Query groupquery = firestoreDb.Collection("Group");
                QuerySnapshot documentSnapshots = await groupquery.GetSnapshotAsync();
                List<GroupModel> lstGroup = new List<GroupModel>();

                foreach (DocumentSnapshot item in documentSnapshots.Documents)
                {
                    if (item.Exists)
                    {
                        Dictionary<string, object> group = item.ToDictionary();
                        string jsonGroup = JsonConvert.SerializeObject(group);
                        groupModel = JsonConvert.DeserializeObject<GroupModel>(jsonGroup);

                        groupModel.Id = item.Id.ToString();
                        lstGroup.Add(groupModel);
                    }
                }
                List<GroupModel> sortGroupList = lstGroup.OrderBy(x => x.name).ToList();
                return View(sortGroupList.ToPagedList(page,10));
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Search(GroupModel groupModel, string searchId, string searchKod, string searchName, string submit)
        {

            if (HttpContext.Session.GetString("bt_userToken") != null)
            {
                Query groupquery = firestoreDb.Collection("Group");
                QuerySnapshot documentSnapshots = await groupquery.GetSnapshotAsync();
                List<GroupModel> lstGroup = new List<GroupModel>();

                foreach (DocumentSnapshot item in documentSnapshots.Documents)
                {
                    if (item.Exists)
                    {
                        Dictionary<string, object> group = item.ToDictionary();
                        string jsonGroup = JsonConvert.SerializeObject(group);
                        groupModel = JsonConvert.DeserializeObject<GroupModel>(jsonGroup);

                        groupModel.Id = item.Id.ToString();
                        lstGroup.Add(groupModel);
                    }
                }

                List<GroupModel> sortGroupList = null;

                if (submit=="Id" && searchId != null)
                {
                    sortGroupList = lstGroup.Where(x => x.Id.Contains(searchId)).OrderBy(x => x.order).ToList();
                }
                else if (submit=="Kod" && searchKod != null)
                {
                    sortGroupList = lstGroup.Where(x => x.code.Contains(searchKod)).OrderBy(x => x.order).ToList();
                }
                else if (submit=="Name" && searchName != null)
                {
                    sortGroupList = lstGroup.Where(x => x.name.Contains(searchName)).OrderBy(x => x.order).ToList();
                }
                else
                {
                    sortGroupList = lstGroup.OrderBy(x => x.order).ToList();
                }

                return View("Index", sortGroupList.ToPagedList(1, 10));
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }


        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("bt_userToken") != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GroupModel groupModel, IFormFile file0, IFormFile file1, IFormFile file2, IFormFile file3, IFormFile file4)
        {
            if (HttpContext.Session.GetString("bt_userToken") != null)
            {
                if (ModelState.IsValid)
                {
                    string _strToken = HttpContext.Session.GetString("bt_userToken");
                    DocumentReference docRef = await firestoreDb.Collection("Group").AddAsync(groupModel);

                    // Insert Storage
                    string link = null;
                    FileStream fs = null;
                    if (file0 != null && file0.Length > 0)
                    {
                        string path = Path.Combine(_env.ContentRootPath, "upload");
                        string fileName = $"f_{Guid.NewGuid()}-{file0.FileName}";
                        using (var memoyrStream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                        {
                            await file0.CopyToAsync(memoyrStream);
                        }
                        using (fs = new FileStream(Path.Combine(path, fileName), FileMode.Open))
                        {
                            var cancellation = new CancellationTokenSource();
                            var upload = new FirebaseStorage(
                                  cls_keys.BucketFile,
                                   new FirebaseStorageOptions
                                   {
                                       AuthTokenAsyncFactory = () => Task.FromResult(_strToken),
                                       ThrowOnCancel = true
                                   })
                                   .Child("Group")
                                  .Child(docRef.Id)
                                   .Child(fileName)
                                   .PutAsync(fs, cancellation.Token);

                            // error during upload will be thrown when await the task
                            link = await upload;
                        }
                        System.IO.File.Delete(Path.Combine(path, fileName));

                        // Update image path
                        await docRef.UpdateAsync("image0", link);
                    }

                    link = null;
                    fs = null;
                    if (file1 != null && file1.Length > 0)
                    {
                        string path = Path.Combine(_env.ContentRootPath, "upload");
                        string fileName = $"f_{Guid.NewGuid()}-{file1.FileName}";
                        using (var memoyrStream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                        {
                            await file1.CopyToAsync(memoyrStream);
                        }
                        using (fs = new FileStream(Path.Combine(path, fileName), FileMode.Open))
                        {
                            var cancellation = new CancellationTokenSource();
                            var upload = new FirebaseStorage(
                                  cls_keys.BucketFile,
                                   new FirebaseStorageOptions
                                   {
                                       AuthTokenAsyncFactory = () => Task.FromResult(_strToken),
                                       ThrowOnCancel = true
                                   })
                                   .Child("Group")
                                  .Child(docRef.Id)
                                   .Child(fileName)
                                   .PutAsync(fs, cancellation.Token);

                            // error during upload will be thrown when await the task
                            link = await upload;
                        }
                        System.IO.File.Delete(Path.Combine(path, fileName));

                        // Update image path
                        await docRef.UpdateAsync("image1", link);
                    }

                    link = null;
                    fs = null;
                    if (file2 != null && file2.Length > 0)
                    {
                        string path = Path.Combine(_env.ContentRootPath, "upload");
                        string fileName = $"f_{Guid.NewGuid()}-{file2.FileName}";
                        using (var memoyrStream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                        {
                            await file2.CopyToAsync(memoyrStream);
                        }
                        using (fs = new FileStream(Path.Combine(path, fileName), FileMode.Open))
                        {
                            var cancellation = new CancellationTokenSource();
                            var upload = new FirebaseStorage(
                                  cls_keys.BucketFile,
                                   new FirebaseStorageOptions
                                   {
                                       AuthTokenAsyncFactory = () => Task.FromResult(_strToken),
                                       ThrowOnCancel = true
                                   })
                                   .Child("Group")
                                  .Child(docRef.Id)
                                   .Child(fileName)
                                   .PutAsync(fs, cancellation.Token);

                            // error during upload will be thrown when await the task
                            link = await upload;
                        }
                        System.IO.File.Delete(Path.Combine(path, fileName));

                        // Update image path
                        await docRef.UpdateAsync("image2", link);
                    }

                    link = null;
                    fs = null;
                    if (file3 != null && file3.Length > 0)
                    {
                        string path = Path.Combine(_env.ContentRootPath, "upload");
                        string fileName = $"f_{Guid.NewGuid()}-{file3.FileName}";
                        using (var memoyrStream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                        {
                            await file3.CopyToAsync(memoyrStream);
                        }
                        using (fs = new FileStream(Path.Combine(path, fileName), FileMode.Open))
                        {
                            var cancellation = new CancellationTokenSource();
                            var upload = new FirebaseStorage(
                                  cls_keys.BucketFile,
                                   new FirebaseStorageOptions
                                   {
                                       AuthTokenAsyncFactory = () => Task.FromResult(_strToken),
                                       ThrowOnCancel = true
                                   })
                                   .Child("Group")
                                  .Child(docRef.Id)
                                   .Child(fileName)
                                   .PutAsync(fs, cancellation.Token);

                            // error during upload will be thrown when await the task
                            link = await upload;
                        }
                        System.IO.File.Delete(Path.Combine(path, fileName));

                        // Update image path
                        await docRef.UpdateAsync("image3", link);
                    }

                    link = null;
                    fs = null;
                    if (file4 != null && file4.Length > 0)
                    {
                        string path = Path.Combine(_env.ContentRootPath, "upload");
                        string fileName = $"f_{Guid.NewGuid()}-{file4.FileName}";
                        using (var memoyrStream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                        {
                            await file4.CopyToAsync(memoyrStream);
                        }
                        using (fs = new FileStream(Path.Combine(path, fileName), FileMode.Open))
                        {
                            var cancellation = new CancellationTokenSource();
                            var upload = new FirebaseStorage(
                                  cls_keys.BucketFile,
                                   new FirebaseStorageOptions
                                   {
                                       AuthTokenAsyncFactory = () => Task.FromResult(_strToken),
                                       ThrowOnCancel = true
                                   })
                                   .Child("Group")
                                  .Child(docRef.Id)
                                   .Child(fileName)
                                   .PutAsync(fs, cancellation.Token);

                            // error during upload will be thrown when await the task
                            link = await upload;
                        }
                        System.IO.File.Delete(Path.Combine(path, fileName));

                        // Update image path
                        await docRef.UpdateAsync("image4", link);
                    }

                    return RedirectToAction("Index", "Group");
                }

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public IActionResult Edit(string? id)
        {
            if (HttpContext.Session.GetString("bt_userToken") != null)
            {
                DocumentReference docRef = firestoreDb.Collection("Group").Document(id);
                DocumentSnapshot snapshot = docRef.GetSnapshotAsync().GetAwaiter().GetResult();
                GroupModel groupModel = snapshot.ConvertTo<GroupModel>();
                groupModel.Id = snapshot.Id;
                return View(groupModel);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(GroupModel groupModel, IFormFile file0, IFormFile file1, IFormFile file2, IFormFile file3, IFormFile file4)
        {
            if (HttpContext.Session.GetString("bt_userToken") != null)
            {
                if (ModelState.IsValid)
                {
                    string _strToken = HttpContext.Session.GetString("bt_userToken");
                    DocumentReference colref = firestoreDb.Collection("Group").Document(groupModel.Id);
                    colref.SetAsync(groupModel).GetAwaiter().GetResult();

                    string link = null;
                    FileStream fs = null;

                    string path = null;
                    string fileName = null;

                    if (file0 != null)
                    {

                        if (file0.Length > 0)
                        {

                            path = Path.Combine(_env.ContentRootPath, "upload");
                            fileName = $"f_{Guid.NewGuid()}-{file0.FileName}";

                            using (var memoyrStream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                            {
                                await file0.CopyToAsync(memoyrStream);
                            }
                            using (fs = new FileStream(Path.Combine(path, fileName), FileMode.Open))
                            {
                                var cancellation = new CancellationTokenSource();
                                var upload = new Firebase.Storage.FirebaseStorage(
                                      cls_keys.BucketFile,
                                       new Firebase.Storage.FirebaseStorageOptions
                                       {
                                           AuthTokenAsyncFactory = () => Task.FromResult(_strToken),
                                           ThrowOnCancel = true
                                       })
                                       .Child("Group")
                                      .Child(colref.Id)
                                       .Child(fileName)
                                       .PutAsync(fs, cancellation.Token);

                                // error during upload will be thrown when await the task
                                link = await upload;
                            }
                            System.IO.File.Delete(Path.Combine(path, fileName));

                            // Update image path
                            await colref.UpdateAsync("image0", link);
                        }
                    }
                    if (file1 != null)
                    {
                        if (file1.Length > 0)
                        {
                            link = null;
                            fs = null;
                            path = Path.Combine(_env.ContentRootPath, "upload");
                            fileName = $"f_{Guid.NewGuid()}-{file1.FileName}";
                            using (var memoyrStream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                            {
                                await file1.CopyToAsync(memoyrStream);
                            }
                            using (fs = new FileStream(Path.Combine(path, fileName), FileMode.Open))
                            {
                                var cancellation = new CancellationTokenSource();
                                var upload = new Firebase.Storage.FirebaseStorage(
                                      cls_keys.BucketFile,
                                       new Firebase.Storage.FirebaseStorageOptions
                                       {
                                           AuthTokenAsyncFactory = () => Task.FromResult(_strToken),
                                           ThrowOnCancel = true
                                       })
                                       .Child("Group")
                                      .Child(colref.Id)
                                       .Child(fileName)
                                       .PutAsync(fs, cancellation.Token);

                                // error during upload will be thrown when await the task
                                link = await upload;
                            }
                            System.IO.File.Delete(Path.Combine(path, fileName));

                            // Update image path
                            await colref.UpdateAsync("image1", link);
                        }
                    }
                    if (file2 != null)
                    {
                        if (file2.Length > 0)
                        {
                            link = null;
                            fs = null;
                            path = Path.Combine(_env.ContentRootPath, "upload");
                            fileName = $"f_{Guid.NewGuid()}-{file2.FileName}";
                            using (var memoyrStream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                            {
                                await file2.CopyToAsync(memoyrStream);
                            }
                            using (fs = new FileStream(Path.Combine(path, fileName), FileMode.Open))
                            {
                                var cancellation = new CancellationTokenSource();
                                var upload = new Firebase.Storage.FirebaseStorage(
                                      cls_keys.BucketFile,
                                       new Firebase.Storage.FirebaseStorageOptions
                                       {
                                           AuthTokenAsyncFactory = () => Task.FromResult(_strToken),
                                           ThrowOnCancel = true
                                       })
                                       .Child("Group")
                                      .Child(colref.Id)
                                       .Child(fileName)
                                       .PutAsync(fs, cancellation.Token);

                                // error during upload will be thrown when await the task
                                link = await upload;
                            }
                            System.IO.File.Delete(Path.Combine(path, fileName));

                            // Update image path
                            await colref.UpdateAsync("image2", link);
                        }
                    }
                    if (file3 != null)
                    {
                        if (file3.Length > 0)
                        {
                            link = null;
                            fs = null;
                            path = Path.Combine(_env.ContentRootPath, "upload");
                            fileName = $"f_{Guid.NewGuid()}-{file3.FileName}";
                            using (var memoyrStream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                            {
                                await file3.CopyToAsync(memoyrStream);
                            }
                            using (fs = new FileStream(Path.Combine(path, fileName), FileMode.Open))
                            {
                                var cancellation = new CancellationTokenSource();
                                var upload = new Firebase.Storage.FirebaseStorage(
                                      cls_keys.BucketFile,
                                       new Firebase.Storage.FirebaseStorageOptions
                                       {
                                           AuthTokenAsyncFactory = () => Task.FromResult(_strToken),
                                           ThrowOnCancel = true
                                       })
                                       .Child("Group")
                                      .Child(colref.Id)
                                       .Child(fileName)
                                       .PutAsync(fs, cancellation.Token);

                                // error during upload will be thrown when await the task
                                link = await upload;
                            }
                            System.IO.File.Delete(Path.Combine(path, fileName));

                            // Update image path
                            await colref.UpdateAsync("image3", link);
                        }
                    }
                    if (file4 != null)
                    {
                        if (file4.Length > 0)
                        {
                            link = null;
                            fs = null;
                            path = Path.Combine(_env.ContentRootPath, "upload");
                            fileName = $"f_{Guid.NewGuid()}-{file4.FileName}";
                            using (var memoyrStream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                            {
                                await file4.CopyToAsync(memoyrStream);
                            }
                            using (fs = new FileStream(Path.Combine(path, fileName), FileMode.Open))
                            {
                                var cancellation = new CancellationTokenSource();
                                var upload = new Firebase.Storage.FirebaseStorage(
                                      cls_keys.BucketFile,
                                       new Firebase.Storage.FirebaseStorageOptions
                                       {
                                           AuthTokenAsyncFactory = () => Task.FromResult(_strToken),
                                           ThrowOnCancel = true
                                       })
                                       .Child("Group")
                                      .Child(colref.Id)
                                       .Child(fileName)
                                       .PutAsync(fs, cancellation.Token);

                                // error during upload will be thrown when await the task
                                link = await upload;
                            }
                            System.IO.File.Delete(Path.Combine(path, fileName));

                            // Update image path
                            await colref.UpdateAsync("image4", link);
                        }
                    }

                    return RedirectToAction("Index", "Group");
                }
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        public IActionResult Delete(string? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            if (HttpContext.Session.GetString("bt_userToken") != null)
            {
                DocumentReference docRef = firestoreDb.Collection("Group").Document(id);
                DocumentSnapshot snapshot = docRef.GetSnapshotAsync().GetAwaiter().GetResult();
                GroupModel groupModel = snapshot.ConvertTo<GroupModel>();
                groupModel.Id = snapshot.Id;
                return View(groupModel);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(UserModel groupModel)
        {
            DocumentReference usrRef = firestoreDb.Collection("Group").Document(groupModel.Id);
            usrRef.DeleteAsync().GetAwaiter().GetResult();
            return RedirectToAction("Index", "Group");
        }

        public IActionResult Details(string id)
        {
            if (HttpContext.Session.GetString("bt_userToken") != null)
            {
                if (id == null)
                {
                    return BadRequest();
                }

                DocumentReference docRef = firestoreDb.Collection("Group").Document(id);
                DocumentSnapshot snapshot = docRef.GetSnapshotAsync().GetAwaiter().GetResult();
                if (snapshot.Exists)
                {
                    GroupModel grp = snapshot.ConvertTo<GroupModel>();
                    grp.Id = snapshot.Id;
                    return View(grp);
                }
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }
    }
}
