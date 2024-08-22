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
    public class QuizsController : ControllerBase
    {
        private readonly DBContext _context;

        public QuizsController(DBContext context)
        {
            _context = context;
        }

        // GET: api/Quizs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Quiz>>> GetQuizs()
        {
            return await _context.Quizs
               .Include(q => q.QuizDetails)
               .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Quiz>> GetQuiz(int id)
        {
            var quiz = await _context.Quizs
                .Include(q => q.QuizDetails)
                .FirstOrDefaultAsync(q => q.QuizId == id);

            if (quiz == null)
            {
                return NotFound();
            }

            return quiz;
        }

        [HttpGet("quiz-and-accounts")]
        public async Task<ActionResult<IEnumerable<QuizAndAccountsDto>>> GetQuizsAndAccounts()
        {
            var quizsAndAccounts = await _context.Quizs
                .Include(q => q.QuizDetails)
                .Select(q => new QuizAndAccountsDto
                {
                    QuizId = q.QuizId,
                    Title = q.QuizTitle,
                    Accounts = q.AccountQuizzes
                        .Select(aq => new AccountQuizDto
                        {
                            Username = aq.Account.Username,
                            Point = (int) aq.Point
                        })
                        .ToList()
                })
                .ToListAsync();

            if (quizsAndAccounts == null || quizsAndAccounts.Count == 0)
            {
                return NotFound(new { Message = "No quizs found." });
            }

            return Ok(quizsAndAccounts);
        }

        public class QuizAndAccountsDto
        {
            public int QuizId { get; set; }
            public string Title { get; set; }
            public List<AccountQuizDto> Accounts { get; set; }
        }

        [HttpGet("{id}/accounts")]
        public async Task<ActionResult<IEnumerable<AccountQuizDto>>> GetAccountsForQuiz(int id)
        {
            var accountQuizs = await _context.AccountQuizzes
                .Where(aq => aq.QuizId == id)
                .Include(aq => aq.Account) // Include Account to get Username
                .Select(aq => new AccountQuizDto
                {
                    Username = aq.Account.Username,
                    Point = (int) aq.Point
                })
                .ToListAsync();

            if (accountQuizs == null || accountQuizs.Count == 0)
            {
                return NotFound(new { Message = "No accounts found for this quiz." });
            }

            return Ok(accountQuizs);
        }

       
        public class AccountQuizDto
        {
            public string Username { get; set; }
            public int Point { get; set; }
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitQuizResult([FromBody] QuizSubmissionModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var accountQuiz = new AccountQuiz
            {
                AccountId = model.AccountId,
                QuizId = model.QuizId,
                Point = model.Point
            };

            _context.AccountQuizzes.Add(accountQuiz);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Quiz result submitted successfully." });
        }

        public class QuizSubmissionModel
        {
            public int AccountId { get; set; }
            public int QuizId { get; set; }
            public int Point { get; set; }
        }


        [HttpPost]
        public async Task<ActionResult<Quiz>> PostQuiz([FromBody] QuizDto quizDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Tạo một đối tượng Quiz từ dữ liệu trong DTO
            var quiz = new Quiz
            {
                QuizTitle = quizDto.QuizTitle,
                QuizDesc = quizDto.QuizDesc,
                CreateBy = quizDto.CreateBy
            };

            // Tạo danh sách các đối tượng QuizDetail từ dữ liệu trong DTO
            var quizDetails = new List<QuizDetail>();
            foreach (var detailDto in quizDto.QuizDetails)
            {
                var detail = new QuizDetail
                {
                    Question = detailDto.Question,
                    Option1 = detailDto.Option1,
                    Option2 = detailDto.Option2,
                    Option3 = detailDto.Option3,
                    Option4 = detailDto.Option4,
                    CorrectAnswer = detailDto.CorrectAnswer,
                    Avatar = detailDto.Avatar
                };
                quizDetails.Add(detail);
            }

            // Gán danh sách QuizDetail cho Quiz
            quiz.QuizDetails = quizDetails;

            try
            {
                // Thêm bài Quiz mới vào cơ sở dữ liệu
                _context.Quizs.Add(quiz);
                await _context.SaveChangesAsync();

                // Trả về kết quả thành công
                return CreatedAtAction("GetQuiz", new { id = quiz.QuizId }, quiz);
            }
            catch (Exception ex)
            {
                // Xử lý các lỗi và trả về thông báo lỗi nếu cần
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        public class QuizDto
        {
            public string QuizTitle { get; set; }
            public string QuizDesc { get; set; }
            public string CreateBy { get; set; }
            public List<QuizDetailDto> QuizDetails { get; set; }
        }

        public class QuizDetailDto
        {
            public string Question { get; set; }
            public string Option1 { get; set; }
            public string Option2 { get; set; }
            public string Option3 { get; set; }
            public string Option4 { get; set; }
            public int CorrectAnswer { get; set; }
            public string Avatar { get; set; }
        }



        [HttpPut("{id}")]
        public async Task<IActionResult> PutQuiz(int id, [FromBody] Quiz quiz)
        {
            if (id != quiz.QuizId)
            {
                return BadRequest();
            }

            _context.Entry(quiz).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QuizExists(id))
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuiz(int id)
        {
            var quiz = await _context.Quizs.FindAsync(id);
            if (quiz == null)
            {
                return NotFound();
            }

            _context.Quizs.Remove(quiz);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool QuizExists(int id)
        {
            return _context.Quizs.Any(e => e.QuizId == id);
        }


        [HttpPost("{quizId}/quizDetails")]
        public async Task<ActionResult<QuizDetail>> PostQuizDetail(int quizId, [FromBody] QuizDetailDto quizDetailDto)
        {
            // Kiểm tra xem quiz có tồn tại không
            var quiz = await _context.Quizs.FindAsync(quizId);
            if (quiz == null)
            {
                return NotFound("Quiz not found");
            }

            // Tạo một đối tượng QuizDetail từ dữ liệu trong DTO
            var quizDetail = new QuizDetail
            {
                QuizId = quizId,
                Question = quizDetailDto.Question,
                Option1 = quizDetailDto.Option1,
                Option2 = quizDetailDto.Option2,
                Option3 = quizDetailDto.Option3,
                Option4 = quizDetailDto.Option4,
                CorrectAnswer = quizDetailDto.CorrectAnswer,
                Avatar = quizDetailDto.Avatar
            };

            try
            {
                // Thêm quiz detail vào danh sách QuizDetails của quiz
                quiz.QuizDetails.Add(quizDetail);
                await _context.SaveChangesAsync();

                // Trả về kết quả thành công
                return CreatedAtAction("PostQuizDetail", new { quizId = quizId, id = quizDetail.QuizDetailId }, quizDetail);
            }
            catch (Exception ex)
            {
                // Xử lý các lỗi và trả về thông báo lỗi nếu cần
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }

}
