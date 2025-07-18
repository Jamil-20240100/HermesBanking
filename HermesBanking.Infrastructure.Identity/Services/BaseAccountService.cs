﻿using HermesBanking.Core.Application.DTOs.Email;
using HermesBanking.Core.Application.DTOs.User;
using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace HermesBanking.Infrastructure.Identity.Services
{
    public abstract class BaseAccountService : IBaseAccountService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService;
        protected BaseAccountService(UserManager<AppUser> userManager, IEmailService emailService)
        {
            _userManager = userManager;
            _emailService = emailService;
        }

        public virtual async Task<RegisterResponseDto> RegisterUser(SaveUserDto saveDto, string? origin, bool? isApi = false)
        {
            RegisterResponseDto response = new()
            {
                Email = "",
                Id = "",
                LastName = "",
                Name = "",
                UserName = "",
                HasError = false,
                Errors = []
            };

            var userWithSameUserName = await _userManager.FindByNameAsync(saveDto.UserName);
            if (userWithSameUserName != null)
            {
                response.HasError = true;
                response.Errors.Add($"this username: {saveDto.UserName} is already taken.");
                return response;
            }

            var userWithSameEmail = await _userManager.FindByEmailAsync(saveDto.Email);
            if (userWithSameEmail != null)
            {
                response.HasError = true;
                response.Errors.Add($"this email: {saveDto.Email} is already taken.");
                return response;
            }

            AppUser user = new AppUser()
            {
                Name = saveDto.Name,
                LastName = saveDto.LastName,
                Email = saveDto.Email,
                UserName = saveDto.UserName,
                EmailConfirmed = false,
                UserId = saveDto.UserId,
                InitialAmount = saveDto.InitialAmount,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, saveDto.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, saveDto.Role);
                if (isApi != null && !isApi.Value)
                {
                    string verificationUri = await GetVerificationEmailUri(user, origin ?? "");
                    await _emailService.SendAsync(new EmailRequestDto()
                    {
                        To = saveDto.Email,
                        HtmlBody = $"Please confirm your account visiting this URL {verificationUri}",
                        Subject = "Confirm registration"
                    });
                }
                else
                {
                    string? verificationToken = await GetVerificationEmailToken(user);
                    await _emailService.SendAsync(new EmailRequestDto()
                    {
                        To = saveDto.Email,
                        HtmlBody = $"Please confirm your account use this token {verificationToken}",
                        Subject = "Confirm registration"
                    });
                }

                var rolesList = await _userManager.GetRolesAsync(user);

                response.Id = user.Id;
                response.Email = user.Email ?? "";
                response.UserName = user.UserName ?? "";
                response.Name = user.Name;
                response.LastName = user.LastName;
                response.IsVerified = user.EmailConfirmed;
                response.Roles = rolesList.ToList();

                return response;
            }
            else
            {
                response.HasError = true;
                response.Errors.AddRange(result.Errors.Select(s => s.Description).ToList());
                return response;
            }
        }

        public virtual async Task<EditResponseDto> EditUser(SaveUserDto saveDto, string? origin, bool? isCreated = false, bool? isApi = false)
        {
            bool isNotcreated = !isCreated ?? false;
            EditResponseDto response = new()
            {
                Email = "",
                Id = "",
                LastName = "",
                Name = "",
                UserName = "",
                HasError = false,
                Errors = []
            };

            var userWithSameUserName = await _userManager.Users.FirstOrDefaultAsync(w => w.UserName == saveDto.UserName && w.Id != saveDto.Id);
            if (userWithSameUserName != null)
            {
                response.HasError = true;
                response.Errors.Add($"this username: {saveDto.UserName} is already taken.");
                return response;
            }

            var userWithSameEmail = await _userManager.Users.FirstOrDefaultAsync(w => w.Email == saveDto.Email && w.Id != saveDto.Id);
            if (userWithSameEmail != null)
            {
                response.HasError = true;
                response.Errors.Add($"this email: {saveDto.Email} is already taken.");
                return response;
            }

            var user = await _userManager.FindByIdAsync(saveDto.Id);

            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add($"There is no acccount registered with this user");
                return response;
            }

            user.Name = saveDto.Name;
            user.LastName = saveDto.LastName;
            user.UserName = saveDto.UserName;
            user.EmailConfirmed = user.EmailConfirmed && user.Email == saveDto.Email;
            user.Email = saveDto.Email;
            user.UserId = saveDto.UserId;
            user.InitialAmount = saveDto.InitialAmount;
            user.IsActive = saveDto.IsActive;

            if (!string.IsNullOrWhiteSpace(saveDto.Password) && isNotcreated)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resultChange = await _userManager.ResetPasswordAsync(user, token, saveDto.Password);

                if (resultChange != null && !resultChange.Succeeded)
                {
                    response.HasError = true;
                    response.Errors.AddRange(resultChange.Errors.Select(s => s.Description).ToList());
                    return response;
                }
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                var rolesList = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, rolesList.ToList());

                await _userManager.AddToRoleAsync(user, saveDto.Role);


                if (!user.EmailConfirmed && isNotcreated)
                {
                    if (isApi != null && !isApi.Value)
                    {
                        string verificationUri = await GetVerificationEmailUri(user, origin ?? "");
                        await _emailService.SendAsync(new EmailRequestDto()
                        {
                            To = saveDto.Email,
                            HtmlBody = $"Please confirm your account visiting this URL {verificationUri}",
                            Subject = "Confirm registration"
                        });
                    }
                    else
                    {
                        string? verificationToken = await GetVerificationEmailToken(user);
                        await _emailService.SendAsync(new EmailRequestDto()
                        {
                            To = saveDto.Email,
                            HtmlBody = $"Please confirm your account use this token {verificationToken}",
                            Subject = "Confirm registration"
                        });
                    }
                }

                var updatedRolesList = await _userManager.GetRolesAsync(user);

                response.Id = user.Id;
                response.Email = user.Email ?? "";
                response.UserName = user.UserName ?? "";
                response.Name = user.Name;
                response.LastName = user.LastName;
                response.IsVerified = user.EmailConfirmed;
                response.Roles = updatedRolesList.ToList();

                return response;
            }
            else
            {
                response.HasError = true;
                response.Errors.AddRange(result.Errors.Select(s => s.Description).ToList());
                return response;
            }
        }

        public virtual async Task<UserResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request, bool? isApi = false)
        {
            UserResponseDto response = new() { HasError = false, Errors = [] };

            var user = await _userManager.FindByNameAsync(request.UserName);

            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add($"There is no acccount registered with this username {request.UserName}");
                return response;
            }

            user.EmailConfirmed = false;
            await _userManager.UpdateAsync(user);

            if (isApi != null && !isApi.Value)
            {
                var resetUri = await GetResetPasswordUri(user, request.Origin ?? "");
                await _emailService.SendAsync(new EmailRequestDto()
                {
                    To = user.Email,
                    HtmlBody = $"Please reset your password account visiting this URL {resetUri}",
                    Subject = "Reset password"
                });
            }
            else
            {
                string? resetToken = await GetResetPasswordToken(user);
                await _emailService.SendAsync(new EmailRequestDto()
                {
                    To = user.Email,
                    HtmlBody = $"Please reset your password account use this token {resetToken}",
                    Subject = "Reset password"
                });
            }

            return response;
        }

        public virtual async Task<UserResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            UserResponseDto response = new() { HasError = false, Errors = [] };

            var user = await _userManager.FindByIdAsync(request.UserId);

            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add($"There is no acccount registered with this user");
                return response;
            }

            var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
            var result = await _userManager.ResetPasswordAsync(user, token, request.Password);
            if (!result.Succeeded)
            {
                response.HasError = true;
                response.Errors.AddRange(result.Errors.Select(s => s.Description).ToList());
                return response;
            }

            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            return response;
        }

        public virtual async Task<UserResponseDto> DeleteAsync(string id)
        {
            UserResponseDto response = new() { HasError = false, Errors = [] };
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add($"There is no acccount registered with this user");
                return response;
            }

            await _userManager.DeleteAsync(user);

            return response;
        }

        public virtual async Task<UserDto?> GetUserByEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return null;
            }

            var rolesList = await _userManager.GetRolesAsync(user);

            var userDto = new UserDto()
            {
                Id = user.Id,
                Email = user.Email ?? "",
                LastName = user.LastName,
                Name = user.Name,
                UserName = user.UserName ?? "",
                isVerified = user.EmailConfirmed,
                Role = rolesList.FirstOrDefault() ?? "",
                UserId = user.UserId,
                InitialAmount = user.InitialAmount,
                IsActive = user.IsActive,
            };

            return userDto;
        }

        public virtual async Task<UserDto?> GetUserById(string Id)
        {
            var user = await _userManager.FindByIdAsync(Id);

            if (user == null)
            {
                return null;
            }

            var rolesList = await _userManager.GetRolesAsync(user);

            var userDto = new UserDto()
            {
                Id = user.Id,
                Email = user.Email ?? "",
                LastName = user.LastName,
                Name = user.Name,
                UserName = user.UserName ?? "",
                isVerified = user.EmailConfirmed,
                Role = rolesList.FirstOrDefault() ?? "",
                UserId = user.UserId,
                InitialAmount = user.InitialAmount,
                IsActive = user.IsActive,
            };

            return userDto;
        }

        public virtual async Task<UserDto?> GetUserByUserName(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);

            if (user == null)
            {
                return null;
            }

            var rolesList = await _userManager.GetRolesAsync(user);

            var userDto = new UserDto()
            {
                Id = user.Id,
                Email = user.Email ?? "",
                LastName = user.LastName,
                Name = user.Name,
                UserName = user.UserName ?? "",
                isVerified = user.EmailConfirmed,
                Role = rolesList.FirstOrDefault() ?? "",
                UserId = user.UserId,
                InitialAmount = user.InitialAmount,
                IsActive = user.IsActive,
            };

            return userDto;
        }

        public virtual async Task<List<UserDto>> GetAllUser(bool? isActive = true)
        {
            List<UserDto> listUsersDTOs = [];

            var users = _userManager.Users;

            if (isActive != null && isActive == true)
            {
                users = users.Where(w => w.EmailConfirmed);
            }

            var listUser = await users.ToListAsync();

            foreach (var item in listUser)
            {
                var roleList = await _userManager.GetRolesAsync(item);

                listUsersDTOs.Add(new UserDto()
                {
                    Id = item.Id,
                    Email = item.Email ?? "",
                    LastName = item.LastName,
                    Name = item.Name,
                    UserName = item.UserName ?? "",
                    isVerified = item.EmailConfirmed,
                    Role = roleList.FirstOrDefault() ?? "",
                    UserId = item.UserId,
                    InitialAmount = item.InitialAmount,
                    IsActive = item.IsActive,
                });
            }

            return listUsersDTOs;
        }

        public virtual async Task<List<UserDto>> GetAllUserByRole(string role, bool? isActive = true)
        {
            List<UserDto> listUsersDTOs = [];

            var users = _userManager.Users;

            if (isActive != null && isActive == true)
                users = users.Where(w => w.EmailConfirmed);

            var listUser = await users.ToListAsync();

            foreach (var item in listUser)
            {
                var roleList = await _userManager.GetRolesAsync(item);

                var actualRole = roleList.FirstOrDefault();

                if (actualRole == role)
                {
                    listUsersDTOs.Add(new UserDto()
                    {
                        Id = item.Id,
                        Email = item.Email ?? "",
                        LastName = item.LastName,
                        Name = item.Name,
                        UserName = item.UserName ?? "",
                        isVerified = item.EmailConfirmed,
                        Role = actualRole ?? "",
                        UserId = item.UserId,
                        InitialAmount = item.InitialAmount,
                        IsActive = item.IsActive,
                    });
                }
            }
            return listUsersDTOs;
        }

        public virtual async Task<List<UserDto>> GetAllActiveUserByRole(string role, bool? isActive = true)
        {
            List<UserDto> listUsersDTOs = [];

            var users = _userManager.Users;

            if (isActive != null && isActive == true)
                users = users.Where(w => w.EmailConfirmed && w.IsActive == true);

            var listUser = await users.ToListAsync();

            foreach (var item in listUser)
            {
                var roleList = await _userManager.GetRolesAsync(item);

                var actualRole = roleList.FirstOrDefault();

                if (actualRole == role)
                {
                    listUsersDTOs.Add(new UserDto()
                    {
                        Id = item.Id,
                        Email = item.Email ?? "",
                        LastName = item.LastName,
                        Name = item.Name,
                        UserName = item.UserName ?? "",
                        isVerified = item.EmailConfirmed,
                        Role = actualRole ?? "",
                        UserId = item.UserId,
                        InitialAmount = item.InitialAmount,
                        IsActive = item.IsActive,
                    });
                }
            }
            return listUsersDTOs;
        }

        public virtual async Task<UserResponseDto> ConfirmAccountAsync(string userId, string token)
        {
            UserResponseDto response = new() { HasError = false, Errors = [] };

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                response.Message = "There is no acccount registered with this user";
                response.HasError = true;
                return response;
            }

            token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                response.Message = $"Account confirmed for {user.Email}. You can now use the app";
                response.HasError = false;
                return response;
            }
            else
            {
                response.Message = $"An error occurred while confirming this email {user.Email}";
                response.HasError = true;
                return response;
            }
        }

        public virtual async Task<UserResponseDto> ChangeStatusAsync(string userId, bool status)
        {
            UserResponseDto response = new() { HasError = false, Errors = [] };
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add($"User with ID {userId} not found.");
                return response;
            }

            user.IsActive = status;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                response.HasError = true;
                response.Errors.AddRange(result.Errors.Select(s => s.Description).ToList());
            }

            return response;
        }

        public virtual async Task<PaginationDto<UserDto>> GetPagedUsersAsync(int page = 1, int pageSize = 20, string? rol = null)
        {
            var query = _userManager.Users.AsQueryable();

            var usersWithRoles = await _userManager.Users.ToListAsync();
            var filteredUsers = new List<AppUser>();

            foreach (var user in usersWithRoles)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (!roles.Contains("comercio") && (rol == null || roles.Contains(rol)))
                {
                    filteredUsers.Add(user);
                }
            }

            var orderedUsers = filteredUsers.OrderByDescending(u => u.Id).ToList();

            int totalUsers = orderedUsers.Count;
            int totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);
            page = Math.Clamp(page, 1, totalPages);

            var usersPage = orderedUsers.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var userDtos = new List<UserDto>();
            foreach (var user in usersPage)
            {
                var rolesList = await _userManager.GetRolesAsync(user);
                userDtos.Add(new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName ?? "",
                    Name = user.Name,
                    LastName = user.LastName,
                    Email = user.Email ?? "",
                    Role = rolesList.FirstOrDefault() ?? "",
                    UserId = user.UserId,
                    IsActive = user.IsActive,
                    isVerified = user.EmailConfirmed
                });
            }

            return new PaginationDto<UserDto>
            {
                Data = userDtos,
                Paginacion = new PaginationInfoDto
                {
                    PaginaActual = page,
                    TotalPaginas = totalPages,
                    TotalUsuarios = totalUsers
                }
            };
        }

        public async Task<PaginationDto<UserDto>> GetPagedCommerceUsersAsync(int page = 1, int pageSize = 20, string? rol = null)
        {
            var usersWithRoles = await _userManager.Users.ToListAsync();
            var filteredUsers = new List<AppUser>();

            foreach (var user in usersWithRoles)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("comercio") && (rol == null || roles.Contains(rol)))
                {
                    filteredUsers.Add(user);
                }
            }

            var orderedUsers = filteredUsers.OrderByDescending(u => u.Id).ToList();

            int totalUsers = orderedUsers.Count;
            int totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

            if (totalPages == 0)
                totalPages = 1;

            page = Math.Clamp(page, 1, totalPages);

            var usersPage = orderedUsers.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var userDtos = new List<UserDto>();
            foreach (var user in usersPage)
            {
                var rolesList = await _userManager.GetRolesAsync(user);
                userDtos.Add(new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName ?? "",
                    Name = user.Name,
                    LastName = user.LastName,
                    Email = user.Email ?? "",
                    Role = rolesList.FirstOrDefault() ?? "",
                    UserId = user.UserId,
                    IsActive = user.IsActive,
                    isVerified = user.EmailConfirmed
                });
            }

            return new PaginationDto<UserDto>
            {
                Data = userDtos,
                Paginacion = new PaginationInfoDto
                {
                    PaginaActual = page,
                    TotalPaginas = totalPages,
                    TotalUsuarios = totalUsers
                }
            };
        }



        #region "Protected methods"

        protected async Task<string> GetVerificationEmailUri(AppUser user, string origin)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var route = "Login/ConfirmEmail";
            var completeUrl = new Uri(string.Concat(origin, "/", route));// origin = https://localhost:58296 route=Login/ConfirmEmail
            var verificationUri = QueryHelpers.AddQueryString(completeUrl.ToString(), "userId", user.Id);
            verificationUri = QueryHelpers.AddQueryString(verificationUri.ToString(), "token", token);

            return verificationUri;
        }

        protected async Task<string?> GetVerificationEmailToken(AppUser user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            return token;
        }

        protected async Task<string> GetResetPasswordUri(AppUser user, string origin)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var route = "Login/ResetPassword";
            var completeUrl = new Uri(string.Concat(origin, "/", route));// origin = https://localhost:58296 route=Login/ConfirmEmail
            var resetUri = QueryHelpers.AddQueryString(completeUrl.ToString(), "userId", user.Id);
            resetUri = QueryHelpers.AddQueryString(resetUri.ToString(), "token", token);

            return resetUri;
        }

        protected async Task<string?> GetResetPasswordToken(AppUser user)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            return token;
        }

        #endregion
    }
}
