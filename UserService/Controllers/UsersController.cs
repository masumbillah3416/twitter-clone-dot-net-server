﻿using System.Security.Claims;
using Core.Dtos;
using Core.Interfaces;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UserService.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUsersService _usersService;

    private readonly IFollowerService _followerService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UsersController(IUsersService usersService, IHttpContextAccessor httpContextAccessor,IFollowerService followerService)
    {
        _usersService = usersService;
        _httpContextAccessor = httpContextAccessor;
        _followerService = followerService;
    }

    [HttpGet("{id}"),Authorize]
    public async Task<ActionResult<SearchedUserResponseDto?>> GetUserById(string id)
    {
        var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        User? user = await _usersService.GetUserAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        SearchedUserResponseDto userResponseDto = user.AsDtoSearchedUser();
        userResponseDto.Followers = await _usersService.GetFollowerCount(id);
        userResponseDto.Following = await _usersService.GetFollowingCount(id);
        if (userId != null)
        {
            userResponseDto.IsFollowed = await _followerService.IsFollowing(userId, id);
        }
        return Ok(userResponseDto);
    }

    [HttpGet, Authorize(Roles = "admin")]
    public async Task<ActionResult<PaginatedUserResponseDto>> GetPaginatedUsers([FromQuery] int size = 20, [FromQuery] int page =0)
    {
        return Ok(await _usersService.GetPaginatedUsers(size, page));
    }

    [HttpGet("may-follow"), Authorize]
    public async Task<ActionResult<List<UserResponseDto>>> MayFollowUser([FromQuery] int size = 20)
    {
        return Ok(await _usersService.MayFollowUser(size));
    }


    [HttpPut("edit"), Authorize]
    public async Task<ActionResult<UserResponseDto?>> UpdateUser(UserEditDto userEditDto)
    {
        if (_httpContextAccessor.HttpContext != null)
        {
            User? user = await _usersService.GetUserAsync(userEditDto.Id);
            if (user == null)
            {
                return NotFound();
            }
            string? id = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            string role = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Role)?.Value ?? "";
            if (id != user.Id && role != "admin")
            {
                return Unauthorized();
            }
            user.UserName = userEditDto.UserName;
            user.Email = userEditDto.Email;
            user.FirstName = userEditDto.FirstName;
            user.LastName = userEditDto.LastName;
            user.ProfilePictureUrl = userEditDto.ProfilePictureUrl;
            user.CoverPictureUrl = userEditDto.CoverPictureUrl;
            user.Gender = userEditDto.Gender;
            user.DateOfBirth = userEditDto.DateOfBirth;
            user.UpdatedAt = DateTime.Now;
            user.Bio = userEditDto.Bio;
            user.Address = userEditDto.Address;

            await _usersService.UpdateGetUserAsync(user.Id, user);
            return Ok(user.AsDto());
        }
        else
        {
            return Unauthorized();
        }
    }


    [HttpGet("current-user"), Authorize]
    public async Task<ActionResult<UserResponseDto?>> GetCurrentUser()
    {
        UserResponseDto? user = await _usersService.GetAuthUser();
        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }
}
