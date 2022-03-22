using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Sprout.Exam.Business.DataTransferObjects;
using Sprout.Exam.Common.Enums;
using Sprout.Exam.WebApp.Data;
using Microsoft.EntityFrameworkCore;

namespace Sprout.Exam.WebApp.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDb;

        public EmployeesController(ApplicationDbContext applicationDb)
        { 
            _applicationDb = applicationDb;
        }

        /// <summary>
        /// Refactor this method to go through proper layers and fetch from the DB.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = (await _applicationDb.Employee.ToListAsync()).Where(d => !d.IsDeleted);
            return Ok(result);
        }

        /// <summary>
        /// Refactor this method to go through proper layers and fetch from the DB.
        /// </summary>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = (await _applicationDb.Employee.ToListAsync()).Where(d => !d.IsDeleted).FirstOrDefault(x => x.Id == id);
            return Ok(result);
        }

        /// <summary>
        /// Refactor this method to go through proper layers and update changes to the DB.
        /// </summary>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(EditEmployeeDto input)
        {
            var item = (await _applicationDb.Employee.ToListAsync()).Where(d => !d.IsDeleted).FirstOrDefault(x => x.Id == input.Id);
            if (item == null) return NotFound();
            item.FullName = input.FullName;
            item.Tin = input.Tin;
            item.Birthdate = input.Birthdate.ToString("yyyy-MM-dd");
            item.TypeId = input.TypeId;

            _applicationDb.Update(item);
            _applicationDb.SaveChanges();

            return Ok(item);
        }

        /// <summary>
        /// Refactor this method to go through proper layers and insert employees to the DB.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Post(CreateEmployeeDto input)
        {
            var id = (await _applicationDb.Employee.ToListAsync()).Where(d => !d.IsDeleted).Max(m => m.Id) + 1;
            EmployeeDto emp = new()
            {
                Birthdate = input.Birthdate.ToString("yyyy-MM-dd"),
                FullName = input.FullName,
                Tin = input.Tin,
                TypeId = input.TypeId,
                IsDeleted = false
            };

            _applicationDb.Add(emp);
            _applicationDb.SaveChanges();

            return Created($"/api/employees/{id}", id);
        }


        /// <summary>
        /// Refactor this method to go through proper layers and perform soft deletion of an employee to the DB.
        /// </summary>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = (await _applicationDb.Employee.ToListAsync()).Where(d => !d.IsDeleted).FirstOrDefault(x => x.Id == id);
            if (result == null) return NotFound();
            
            _applicationDb.Remove(result);
            _applicationDb.SaveChanges();

            return Ok(id);
        }



        /// <summary>
        /// Refactor this method to go through proper layers and use Factory pattern
        /// </summary>
        /// <param name="id"></param>
        /// <param name="absentDays"></param>
        /// <param name="workedDays"></param>
        /// <returns></returns>
        [HttpPost("{id}/calculate")]
        public async Task<IActionResult> Calculate(int id, [FromBody] Calculate obj)
        {
            var result = (await _applicationDb.Employee.ToListAsync()).Where(d => !d.IsDeleted).FirstOrDefault(x => x.Id == id);
            if (result == null) return NotFound();
            var type = (EmployeeType) result.TypeId;

            switch (type)
            {
                case EmployeeType.Regular:
                    var deduction = obj.AbsentDays > 0 ? (20000.00 / 22) * obj.AbsentDays : 0.00;
                    var tax = 20000 * .12;
                    var finalSalary = (20000.00 - deduction) - tax;
                    return Ok(finalSalary.ToString("#,##0.00"));
                case EmployeeType.Contractual:
                    var salary = 500 * obj.WorkedDays;
                    return Ok(salary.ToString("#,##0.00"));
                default:
                    return NotFound("Employee Type not found");
            }

        }

    }
}
