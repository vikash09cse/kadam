﻿using Core.Abstractions;
using Core.DTOs.App;
using Core.Entities;
using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Infrastructure
{
    public class StudentRepository(DatabaseContext context) : IStudentRepository
    {
        private readonly DatabaseContext _context = context;

        public async Task<bool> CheckDuplicateStudentRegistrationNumber(string registrationNumber, int id)
        {
            var student = await _context.Students.FirstOrDefaultAsync(x => x.StudentRegistratioNumber == registrationNumber && x.Id != id && !x.IsDeleted);
            return student != null;
        }

        public async Task<bool> CheckDuplicateAadhaarNumber(string aadhaarNumber, int id)
        {
            var student = await _context.Students.FirstOrDefaultAsync(x => x.AadhaarCardNumber == aadhaarNumber && x.Id != id && !x.IsDeleted);
            return student != null;
        }

        public async Task<bool> DeleteStudent(int id, int deletedBy)
        {
            var student = await _context.Students.FirstOrDefaultAsync(x => x.Id == id);
            if (student != null)
            {
                student.IsDeleted = true;
                student.DeletedBy = deletedBy;
                student.DeletedDate = DateTime.UtcNow;
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<Student> GetStudent(int id)
        {
            return await _context.Students.FirstOrDefaultAsync(x => x.Id == id) ?? new Student();
        }

        public async Task<IEnumerable<Student>> GetAllStudents()
        {
            return await _context.Students.Where(x => !x.IsDeleted).ToListAsync();
        }

        public async Task<bool> SaveStudent(Student student)
        {
            if (student.Id > 0)
            {
                _context.Students.Update(student);
            }
            else
            {
                _context.Students.Add(student);
            }
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<AppInstitutionDTO>> GetInstitutionsByUserId(int userId)
        {
            using (var connection = _context.Database.GetDbConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);

                using var multi = await connection.QueryMultipleAsync("usp_GetInstitutionByUserId", parameters, commandType: CommandType.StoredProcedure);
                
                var institutions = await multi.ReadAsync<AppInstitutionDTO>();
                var grades = await multi.ReadAsync<AppGradeSectionDTO>();

                var result = institutions.ToList();
                foreach (var institution in result)
                {
                    institution.GradeSections = grades.Where(x => x.InstitutionId == institution.Id).ToList();
                }

                return result;
            }
        }
    }
}
