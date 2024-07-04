
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ProjectName.ControllersExceptions;
using ProjectName.Interfaces;
using ProjectName.Types;

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
            // Step 1: Validate the request payload contains the necessary parameter ("Name").
            if (string.IsNullOrEmpty(request.Name))
            {
                throw new BusinessException("DP-422", "Client Error");
            }

            // Step 2: Validate that the provided parent category ID exists if it's included in the request payload.
            if (request.Parent.HasValue)
            {
                var parentExists = await _dbConnection.ExecuteScalarAsync<bool>(
                    "SELECT COUNT(1) FROM BlogCategories WHERE Id = @Parent",
                    new { Parent = request.Parent.Value });

                if (!parentExists)
                {
                    throw new TechnicalException("DP-404", "Technical Error");
                }
            }

            // Step 3: Create a new blogCategory object with the provided details.
            var blogCategory = new BlogCategory
            {
                Id = Guid.NewGuid(),
                Parent = request.Parent,
                Name = request.Name
            };

            // Step 4: Insert the newly created BlogCategory object to the database BlogCategories table.
            var insertQuery = "INSERT INTO BlogCategories (Id, Parent, Name) VALUES (@Id, @Parent, @Name)";
            var rowsAffected = await _dbConnection.ExecuteAsync(insertQuery, blogCategory);

            // Step 5: If the transaction is successful, return the new BlogCategory ID.
            if (rowsAffected > 0)
            {
                return blogCategory.Id.ToString();
            }
            else
            {
                throw new TechnicalException("DP-500", "Technical Error");
            }
        }

        public async Task<BlogCategory> GetBlogCategory(BlogCategoryRequestDto request)
        {
            // Step 1: Validate that request.payload.Id is not null.
            if (request.Id == Guid.Empty)
            {
                throw new BusinessException("DP-422", "Client Error");
            }

            // Step 2: Fetch the blog category from the database based on the provided category ID.
            var query = "SELECT * FROM BlogCategories WHERE Id = @Id";
            var blogCategory = await _dbConnection.QuerySingleOrDefaultAsync<BlogCategory>(query, new { Id = request.Id });

            // Step 3: If the category exists, return it as the response payload.
            if (blogCategory != null)
            {
                return blogCategory;
            }
            else
            {
                throw new TechnicalException("DP-404", "Technical Error");
            }
        }

        public async Task<string> UpdateBlogCategory(UpdateBlogCategoryDto request)
        {
            // Step 1: Validate that the request payload contains the necessary parameters ("Id" and "Name").
            if (request.Id == Guid.Empty || string.IsNullOrEmpty(request.Name))
            {
                throw new BusinessException("DP-422", "Client Error");
            }

            // Step 2: Fetch the BlogCategory from the database by Id.
            var existingCategory = await _dbConnection.QuerySingleOrDefaultAsync<BlogCategory>(
                "SELECT * FROM BlogCategories WHERE Id = @Id",
                new { Id = request.Id });

            if (existingCategory == null)
            {
                throw new TechnicalException("DP-404", "Technical Error");
            }

            // Step 3: If the category is a subcategory, set Parent to the parent category ID provided.
            existingCategory.Parent = request.Parent;
            existingCategory.Name = request.Name;

            // Step 4: Update the BlogCategory object with the provided changes.
            var updateQuery = "UPDATE BlogCategories SET Parent = @Parent, Name = @Name WHERE Id = @Id";
            var rowsAffected = await _dbConnection.ExecuteAsync(updateQuery, existingCategory);

            // Step 5: If the transaction is successful, return the BlogCategory ID.
            if (rowsAffected > 0)
            {
                return existingCategory.Id.ToString();
            }
            else
            {
                throw new TechnicalException("DP-500", "Technical Error");
            }
        }

        public async Task<bool> DeleteBlogCategory(DeleteBlogCategoryDto request)
        {
            // Step 1: Validate that the request payload contains the necessary parameter ("Id").
            if (request.Id == Guid.Empty)
            {
                throw new BusinessException("DP-422", "Client Error");
            }

            // Step 2: Fetch the BlogCategory from the database by Id.
            var existingCategory = await _dbConnection.QuerySingleOrDefaultAsync<BlogCategory>(
                "SELECT * FROM BlogCategories WHERE Id = @Id",
                new { Id = request.Id });

            if (existingCategory == null)
            {
                throw new TechnicalException("DP-404", "Technical Error");
            }

            // Step 3: Delete the BlogCategory object from the database.
            var deleteQuery = "DELETE FROM BlogCategories WHERE Id = @Id";
            var rowsAffected = await _dbConnection.ExecuteAsync(deleteQuery, new { Id = request.Id });

            // Step 4: If the transaction is successful, return true.
            if (rowsAffected > 0)
            {
                return true;
            }
            else
            {
                throw new TechnicalException("DP-500", "Technical Error");
            }
        }

        public async Task<List<BlogCategory>> GetListBlogCategory(ListBlogCategoryRequestDto request)
        {
            // Step 1: Validate that the request payload contains the necessary parameters ("PageNumber" and "PageSize").
            if (request.PageLimit <= 0 || request.PageOffset < 0)
            {
                throw new BusinessException("DP-422", "Client Error");
            }

            // Step 2: Fetch the list of BlogCategories from the database based on the provided pagination parameters.
            var query = "SELECT * FROM BlogCategories ORDER BY @SortField @SortOrder LIMIT @PageLimit OFFSET @PageOffset";
            var parameters = new
            {
                PageLimit = request.PageLimit,
                PageOffset = request.PageOffset,
                SortField = request.SortField,
                SortOrder = request.SortOrder
            };

            var blogCategories = await _dbConnection.QueryAsync<BlogCategory>(query, parameters);

            // Step 3: If the transaction is successful, return the list of BlogCategories.
            return blogCategories.ToList();
        }
    }
}
