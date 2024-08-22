using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backendV1.Models;

namespace backendV1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizDetailsController : ControllerBase
    {
        private readonly DBContext _context;

        public QuizDetailsController(DBContext context)
        {
            _context = context;
        }

        // GET: api/QuizDetails
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuizDetail>>> GetQuizDetails()
        {
            return await _context.QuizDetails.ToListAsync();
        }

        // GET: api/QuizDetails/5
        [HttpGet("{id}")]
        public async Task<ActionResult<QuizDetail>> GetQuizDetail(int id)
        {
            var quizDetail = await _context.QuizDetails.FindAsync(id);

            if (quizDetail == null)
            {
                return NotFound();
            }

            return quizDetail;
        }

        [HttpGet("ByQuiz/{quizId}")]
        public async Task<ActionResult<IEnumerable<QuizDetail>>> GetQuizDetailsByQuizId(int quizId)
        {
            var quizDetails = await _context.QuizDetails.Where(qd => qd.QuizId == quizId).ToListAsync();

            if (quizDetails == null || quizDetails.Count == 0)
            {
                return NotFound();
            }

            return quizDetails;
        }

        // PUT: api/QuizDetails/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutQuizDetail(int id, QuizDetail quizDetail)
        {
            if (id != quizDetail.QuizDetailId)
            {
                return BadRequest();
            }

            _context.Entry(quizDetail).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QuizDetailExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/QuizDetails
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<QuizDetail>> PostQuizDetail(QuizDetail quizDetail)
        {
            _context.QuizDetails.Add(quizDetail);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetQuizDetail", new { id = quizDetail.QuizDetailId }, quizDetail);
        }

        // DELETE: api/QuizDetails/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuizDetail(int id)
        {
            var quizDetail = await _context.QuizDetails.FindAsync(id);
            if (quizDetail == null)
            {
                return NotFound();
            }

            _context.QuizDetails.Remove(quizDetail);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool QuizDetailExists(int id)
        {
            return _context.QuizDetails.Any(e => e.QuizDetailId == id);
        }
    }
}
