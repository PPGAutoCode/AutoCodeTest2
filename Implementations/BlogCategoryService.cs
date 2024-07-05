
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using ProjectName.Interfaces;
using ProjectName.Types;
using ProjectName.ControllersExceptions;

namespace ProjectName.Services
{
    public class BlogCategoryService : IBlogCategoryService
    {
        private readonly IDbConnection _dbConnection;

        public BlogCategoryService(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<string> CreateBlogCategory(CreateBlogCategoryDto request)
        {
            if (string.IsNullOrEmpty(request.Name))
            {
                throw new BusinessException("DP-422", "Client Error");
            }

            if (request.Parent.HasValue)
            {
                var parentExists = await _dbConnection.ExecuteScalarAsync<bool>(
                    "SELECT COUNT(1) FROM BlogCategories WHERE Id = @Parent",
                    new { Parent = request.Parent.Value });

                if (!parentExists)
                {
                    throw new BusinessException("DP-404", "Technical Error");
                }
            }

            var blogCategory = new BlogCategory
            {
                Id = Guid.NewGuid(),
                Parent = request.Parent,
                Name = request.Name
            };

            try
            {
                await _dbConnection.ExecuteAsync(
                    "INSERT INTO BlogCategories (Id, Parent, Name) VALUES (@Id, @Parent, @Name)",
                    blogCategory);

                return blogCategory.Id.ToString();
            }
            catch (Exception)
            {
                throw new TechnicalException("DP-500", "Technical Error");
            }
        }

        public async Task<BlogCategory> GetBlogCategory(BlogCategoryRequestDto request)
        {
            if (request.Id == Guid.Empty)
            {
                throw new BusinessException("DP-422", "Client Error");
            }

            var blogCategory = await _dbConnection.QuerySingleOrDefaultAsync<BlogCategory>(
                "SELECT * FROM BlogCategories WHERE Id = @Id",
                new { Id = request.Id });

            if (blogCategory == null)
            {
                throw new BusinessException("DP-404", "Technical Error");
            }

            return blogCategory;
        }

        public async Task<string> UpdateBlogCategory(UpdateBlogCategoryDto request)
        {
            if (request.Id == Guid.Empty || string.IsNullOrEmpty(request.Name))
            {
                throw new BusinessException("DP-422", "Client Error");
            }

            var blogCategory = await _dbConnection.QuerySingleOrDefaultAsync<BlogCategory>(
                "SELECT * FROM BlogCategories WHERE Id = @Id",
                new { Id = request.Id });

            if (blogCategory == null)
            {
                throw new BusinessException("DP-404", "Technical Error");
            }

            blogCategory.Parent = request.Parent;
            blogCategory.Name = request.Name;

            try
            {
                await _dbConnection.ExecuteAsync(
                    "UPDATE BlogCategories SET Parent = @Parent, Name = @Name WHERE Id = @Id",
                    new { Id = blogCategory.Id, Parent = blogCategory.Parent, Name = blogCategory.Name });

                return blogCategory.Id.ToString();
            }
            catch (Exception)
            {
                throw new TechnicalException("DP-500", "Technical Error");
            }
        }

        public async Task<bool> DeleteBlogCategory(DeleteBlogCategoryDto request)
        {
            if (request.Id == Guid.Empty)
            {
                throw new BusinessException("DP-422", "Client Error");
            }

            var blogCategory = await _dbConnection.QuerySingleOrDefaultAsync<BlogCategory>(
                "SELECT * FROM BlogCategories WHERE Id = @Id",
                new { Id = request.Id });

            if (blogCategory == null)
            {
                throw new BusinessException("DP-404", "Technical Error");
            }

            try
            {
                await _dbConnection.ExecuteAsync(
                    "DELETE FROM BlogCategories WHERE Id = @Id",
                    new { Id = request.Id });

                return true;
            }
            catch (Exception)
            {
                throw new TechnicalException("DP-500", "Technical Error");
            }
        }

        public async Task<List<BlogCategory>> GetListBlogCategory(ListBlogCategoryRequestDto request)
        {
            if (request.PageLimit <= 0 || request.PageOffset < 0 || string.IsNullOrEmpty(request.SortField) || string.IsNullOrEmpty(request.SortOrder))
            {
                throw new BusinessException("DP-422", "Client Error");
            }

            try
            {
                var blogCategories = await _dbConnection.QueryAsync<BlogCategory>(
                    "SELECT * FROM BlogCategories ORDER BY @SortField @SortOrder OFFSET @PageOffset ROWS FETCH NEXT @PageLimit ROWS ONLY",
                    new { PageOffset = request.PageOffset, PageLimit = request.PageLimit, SortField = request.SortField, SortOrder = request.SortOrder });

                return blogCategories.AsList();
            }
            catch (Exception)
            {
                throw new TechnicalException("DP-500", "Technical Error");
            }
        }
    }
}

        public async Task<List<BlogCategory>> GetListBlogCategoryByParent(Guid parentId)
        {
            try
            {
                var blogCategories = await _dbConnection.QueryAsync<BlogCategory>(
                    "SELECT * FROM BlogCategories WHERE Parent = @Parent",
                    new { Parent = parentId });

                return blogCategories.AsList();
            }
            catch (Exception)
            {
                throw new TechnicalException("DP-500", "Technical Error");
            }
        }
    }
}
