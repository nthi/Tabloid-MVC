﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System.Security.Claims;
using TabloidMVC.Models.ViewModels;
using TabloidMVC.Repositories;
using System;
using TabloidMVC.Models;

namespace TabloidMVC.Controllers
{
    [Authorize]
    public class PostController : Controller
    {
        private readonly IPostRepository _postRepository;
        private readonly ICategoryRepository _categoryRepository;

        public PostController(IPostRepository postRepository, ICategoryRepository categoryRepository)
        {
            _postRepository = postRepository;
            _categoryRepository = categoryRepository;
        }
        //In order to conditionally render edit/delete buttons for the logged in user, we need to use GetCurrentUserProfileId in this method.
        //We have created new viewmodel PostUserViewModel()
        //We are using that viewmodel in Index method
        //then in the index view we have an @if loop to check user id and print edit and delete button if they match the post user id, otherwise not.
        public IActionResult Index()
        {
            int userId = GetCurrentUserProfileId();
            var posts = _postRepository.GetAllPublishedPosts();

            PostUserViewModel vm = new PostUserViewModel()
            {
                UserProfileId = userId,
                Posts = posts
            };

            return View(vm);
        }

        public IActionResult Details(int id)
        {
            var post = _postRepository.GetPublishedPostById(id);
            if (post == null)
            {
                int userId = GetCurrentUserProfileId();
                post = _postRepository.GetUserPostById(id, userId);
                if (post == null)
                {
                    return NotFound();
                }
            }
            return View(post);
        }

        public IActionResult Create()
        {
            var vm = new PostCreateViewModel();
            vm.CategoryOptions = _categoryRepository.GetAll();
            return View(vm);
        }

        [HttpPost]
        public IActionResult Create(PostCreateViewModel vm)
        {
            try
            {
                vm.Post.CreateDateTime = DateAndTime.Now;
                vm.Post.IsApproved = true;
                vm.Post.UserProfileId = GetCurrentUserProfileId();

                _postRepository.Add(vm.Post);

                return RedirectToAction("Details", new { id = vm.Post.Id });
            } 
            catch
            {
                vm.CategoryOptions = _categoryRepository.GetAll();
                return View(vm);
            }
        }

        //This method will get all posts by the user who is currently logged in and return the view as a list similar to Posts.
        public IActionResult MyIndex()
        {
            int userId = GetCurrentUserProfileId();

            var myPosts = _postRepository.GetAllUsersPosts(userId);

            return View(myPosts);
        }

        //GET PostController/Edit/id
        //This method will allow user to edit their own posts.
        public IActionResult Edit(int id)
        {
            int userId = GetCurrentUserProfileId();
            Post post = _postRepository.GetUserPostById(id, userId);

            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        //POST PostController/Edit/id
        //This method will allow user to edit their own posts.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Post post)
        {
            try
            {
                _postRepository.UpdatePost(post);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return View(post);
            }
        }

        //GET: PostController/Delete
        //I believe we need to have GetUserPostById for now even though all posts are admin's.
        public IActionResult Delete(int id)
        {
            int userId = GetCurrentUserProfileId();

            Post post = _postRepository.GetUserPostById(id, userId);

            return View(post);
        }

        //POST: PostController/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, Post post)
        {
            try
            {
                _postRepository.DeletePost(id);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return View(post);
            }
        }

        private int GetCurrentUserProfileId()
        {
            string id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.Parse(id);
        }
    }
}
