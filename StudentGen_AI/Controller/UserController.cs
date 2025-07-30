using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private static List<Student> students = new List<Student>();
    private static int nextId = 1;

    [HttpGet]
    public ActionResult<IEnumerable<Student>> GetAll()
    {
        return Ok(students);
    }

    [HttpGet("{id}")]
    public ActionResult<Student> Get(int id)
    {
        var student = students.FirstOrDefault(s => s.Id == id);
        if (student == null) return NotFound();
        return Ok(student);
    }

    [HttpPost]
    public ActionResult<Student> Create(Student student)
    {
        student.Id = nextId++;
        students.Add(student);
        return CreatedAtAction(nameof(Get), new { id = student.Id }, student);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, Student updatedStudent)
    {
        var student = students.FirstOrDefault(s => s.Id == id);
        if (student == null) return NotFound();
        student.Name = updatedStudent.Name;
        student.Age = updatedStudent.Age;
        student.Email = updatedStudent.Email;
        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var student = students.FirstOrDefault(s => s.Id == id);
        if (student == null) return NotFound();
        students.Remove(student);
        return NoContent();
    }
}