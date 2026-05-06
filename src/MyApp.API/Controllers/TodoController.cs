using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.API.Data;
using MyApp.API.Models;

public class TodoController : Controller
{
    private readonly AppDbContext _db;

    public TodoController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var todos = await _db.TodoItems
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return View(todos);
    }

    [HttpPost]
    public async Task<IActionResult> Create(TodoItem todoItem)
    {
        todoItem.CreatedAt = DateTime.UtcNow;
        todoItem.UpdatedAt = null;

        _db.TodoItems.Add(todoItem);
        await _db.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Complete(List<int> ids)
    {
        if (ids.Count == 0)
        {
            return RedirectToAction("Index");
        }

        var items = await _db.TodoItems
            .Where(x => ids.Contains(x.Id))
            .ToListAsync();

        foreach (var item in items)
        {
            item.IsCompleted = true;
            item.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Delete(List<int> ids)
    {
        if (ids.Count == 0)
        {
            return RedirectToAction("Index");
        }

        var items = await _db.TodoItems
            .Where(x => ids.Contains(x.Id))
            .ToListAsync();

        _db.TodoItems.RemoveRange(items);
        await _db.SaveChangesAsync();

        return RedirectToAction("Index");
    }
}
