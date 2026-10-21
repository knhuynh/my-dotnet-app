namespace MyApp.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.API.Data;
using MyApp.API.Models;

public class TodoController : Controller
{
    private readonly AppDbContext _context;

    public TodoController(AppDbContext db)
    {
        _context = db;
    }

    public async Task<IActionResult> Index()
    {
        var todos = await _context.TodoItems
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return View(todos);
    }

    [HttpPost]
    public async Task<IActionResult> Create(TodoItem todoItem)
    {
        todoItem.CreatedAt = DateTime.UtcNow;
        todoItem.UpdatedAt = null;

        _context.TodoItems.Add(todoItem);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Complete(List<int> ids)
    {
        if (ids.Count == 0)
        {
            return RedirectToAction("Index");
        }

        var items = await _context.TodoItems
            .Where(x => ids.Contains(x.Id))
            .ToListAsync();

        foreach (var item in items)
        {
            item.IsCompleted = true;
            item.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Delete(List<int> ids)
    {
        if (ids.Count == 0)
        {
            return RedirectToAction("Index");
        }

        var items = await _context.TodoItems
            .Where(x => ids.Contains(x.Id))
            .ToListAsync();

        _context.TodoItems.RemoveRange(items);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }
}
