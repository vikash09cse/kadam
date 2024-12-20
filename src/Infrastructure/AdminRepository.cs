﻿using Core.Abstractions;
using Core.DTOs;
using Core.DTOs.Users;
using Core.Entities;
using Core.Utilities;
using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Infrastructure
{
    public class AdminRepository(IDbSession db, DatabaseContext context) : IAdminRepository
    {
        private readonly IDbSession _db = db;
        private readonly DatabaseContext _context = context;

        #region "Users"
        public async Task<bool> CheckUserExist(string email, int id)
        {
            var SP = DBConstant.SP.usp_Users;
            var P = new DynamicParameters();
            P.Add(DBConstant.Param.QueryType, 1);
            P.Add(DBConstant.Param.Id, id);
            P.Add(DBConstant.Param.Email, email);
            var rowCount = await _db.Connection
                .ExecuteScalarAsync<int>(SP, P, _db.Transaction, null, CommandType.StoredProcedure);
            return rowCount > 0;
        }

        public async Task<bool> DeleteUser(int id)
        {
            var SP = DBConstant.SP.usp_Users;
            var P = new DynamicParameters();
            P.Add(DBConstant.Param.QueryType, 2);
            P.Add(DBConstant.Param.Id, id);
            var rowEffected = await _db.Connection.ExecuteAsync(SP, P, _db.Transaction, null, CommandType.StoredProcedure);
            return rowEffected > 0;
        }

        public async Task<Users> GetUser(int id)
        {
            var SP = DBConstant.SP.usp_Users;
            var P = new DynamicParameters();
            P.Add(DBConstant.Param.QueryType, 3);
            P.Add(DBConstant.Param.Id, id);
            var _user = await _db.Connection.QueryFirstOrDefaultAsync<Users>(SP, P, _db.Transaction, null, CommandType.StoredProcedure);
            return _user;
        }

        public async Task<IEnumerable<Users>> GetUsers()
        {
            var SP = DBConstant.SP.usp_Users;
            var P = new DynamicParameters();
            P.Add(DBConstant.Param.QueryType, 4);
            var _users = await _db.Connection.QueryAsync<Users>(SP, P, _db.Transaction, null, CommandType.StoredProcedure);
            return _users;
        }
        public async Task<bool> SaveUser(Users user)
        {
            try
            {
                var SP = DBConstant.SP.usp_UpsertUser;
                var P = new DynamicParameters();
                P.Add(DBConstant.Param.Id, user.Id);
                P.Add(DBConstant.Param.Email, user.Email);
                P.Add(DBConstant.Param.UserName, user.UserName);
                P.Add(DBConstant.Param.PasswordHash, user.PasswordHash);
                P.Add(DBConstant.Param.PasswordSalt, user.PasswordSalt);
                P.Add(DBConstant.Param.FirstName, user.FirstName);
                P.Add(DBConstant.Param.LastName, user.LastName);
                P.Add(DBConstant.Param.Phone, user.Phone);
                P.Add(DBConstant.Param.AlternatePhone, user.AlternatePhone);
                P.Add(DBConstant.Param.Gender, user.Gender);
                P.Add(DBConstant.Param.Grade, user.Grade);
                P.Add(DBConstant.Param.Section, user.Section);
                P.Add(DBConstant.Param.GradeSection, user.GradeSection);
                P.Add(DBConstant.Param.DivisionId, user.DivisionId);
                P.Add(DBConstant.Param.RoleId, user.RoleId);
                P.Add(DBConstant.Param.ReporteeRoleId, user.ReporteeRoleId);
                P.Add(DBConstant.Param.UserStatus, user.UserStatus);
                P.Add(DBConstant.Param.ActivityType, user.ActivityType);
                P.Add(DBConstant.Param.DateCreated, user.DateCreated);
                P.Add(DBConstant.Param.CreatedBy, user.CreatedBy);
                P.Add(DBConstant.Param.ModifyDate, user.ModifyDate);
                P.Add(DBConstant.Param.ModifyBy, user.ModifyBy);
                var rowEffected = await _db.Connection.ExecuteAsync(SP, P, _db.Transaction, null, CommandType.StoredProcedure);
                return rowEffected > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
            
        }

        public async Task<UserLoginValidateDTO> ValidateUser(string userName)
        {
            var SP = DBConstant.SP.usp_UserLoginValidate;
            var P = new DynamicParameters();
            P.Add(DBConstant.Param.UserName, userName);
            

            var _user = await _db.Connection.QueryFirstOrDefaultAsync<UserLoginValidateDTO>(SP, P, _db.Transaction, null, CommandType.StoredProcedure);
            return _user ?? new UserLoginValidateDTO();
        }

        public async Task<IEnumerable<UserListDTO>> GetUserList(int pageNumber, int pageSize, string searchTerm)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@SearchTerm", searchTerm);

            var result = await _db.Connection.QueryAsync<UserListDTO>(
                "dbo.usp_GetUserList",
                parameters,
                _db.Transaction,
                null,
                CommandType.StoredProcedure
            );

            return result;
        }


        #endregion

        #region "Division"
        public async Task<bool> SaveDivision(Division division)
        {
            if (division.Id > 0)
            {
                var filterDivision = _context.Divisions.FirstOrDefault(x => x.Id == division.Id);
                if (filterDivision != null)
                {
                    filterDivision.DivisionName = division.DivisionName;
                    filterDivision.DivisionCode = division.DivisionCode;
                    filterDivision.ModifyDate = DateTime.UtcNow;
                    filterDivision.ModifyBy = division.ModifyBy;
                    _context.Divisions.Update(filterDivision);
                }
            }
            else
            {
                _context.Divisions.Add(division);
            }
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Division> GetDivision(int id)
        {
            return await _context.Divisions.FirstOrDefaultAsync(x => x.Id == id) ?? new Division();
        }

        public async Task<IEnumerable<DivisionListDTO>> GetDivisionList(int pageNumber, int pageSize, string searchTerm)
        {
            var storedProcedure = "dbo.usp_DivisionList";
            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", pageNumber);
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@SearchTerm", searchTerm);

            var divisions = await _db.Connection.QueryAsync<DivisionListDTO>(
                storedProcedure,
                parameters,
                _db.Transaction,
                commandType: CommandType.StoredProcedure
            );

            return divisions;
        }

        public async Task<bool> DeleteDivision(int id,int deletedBy)
        {
            var division = await _context.Divisions.FirstOrDefaultAsync(x => x.Id == id);
            if (division != null)
            {
                division.IsDeleted = true;
                division.DeletedBy = deletedBy;
                division.DeletedDate = DateTime.UtcNow;
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<bool> CheckDuplicateDivisionName(string divisionName, int id)
        {
            var division = await _context.Divisions.FirstOrDefaultAsync(x => x.DivisionName == divisionName && x.Id != id);
            return division != null;

        }

        public async Task<bool> CheckDuplicateDivisionCode(string divisionCode, int id)
        {
            var division = await _context.Divisions.FirstOrDefaultAsync(x => x.DivisionCode == divisionCode && x.Id != id);
            return division != null;
        }
        #endregion
    }
}
