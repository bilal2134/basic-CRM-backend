using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRM.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerRepository _repository;
        public CustomerController(ICustomerRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string search = null)
        {
            var allCustomers = await _repository.GetAllAsync();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var lower = search.ToLower();
                allCustomers = allCustomers.Where(c =>
                    (c.Name != null && c.Name.ToLower().Contains(lower)) ||
                    (c.Email != null && c.Email.ToLower().Contains(lower)) ||
                    (c.PhoneNumber != null && c.PhoneNumber.ToLower().Contains(lower))
                );
            }
            var total = allCustomers.Count();
            var customers = allCustomers
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            return Ok(new { data = customers, total });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetById(int id)
        {
            var customer = await _repository.GetByIdAsync(id);
            if (customer == null) return NotFound();
            return Ok(customer);
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] Customer customer)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                await _repository.AddAsync(customer);
                return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] UpdateCustomerDto updateDto)
        {
            if (updateDto == null)
                return BadRequest("No data provided.");
            var customer = await _repository.GetByIdAsync(id);
            if (customer == null)
                return NotFound();
            bool updated = false;
            if (updateDto.Name != null)
            {
                if (string.IsNullOrWhiteSpace(updateDto.Name))
                    return BadRequest("Name cannot be empty.");
                customer.Name = updateDto.Name;
                updated = true;
            }
            if (updateDto.Email != null)
            {
                if (!new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(updateDto.Email))
                    return BadRequest("Invalid email format.");
                customer.Email = updateDto.Email;
                updated = true;
            }
            if (updateDto.PhoneNumber != null)
            {
                if (!new System.ComponentModel.DataAnnotations.PhoneAttribute().IsValid(updateDto.PhoneNumber))
                    return BadRequest("Invalid phone number format.");
                customer.PhoneNumber = updateDto.PhoneNumber;
                updated = true;
            }
            if (!updated)
                return BadRequest("No valid fields provided for update.");
            try
            {
                await _repository.UpdateAsync(customer);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _repository.DeleteAsync(id);
            return NoContent();
        }
    }
}
