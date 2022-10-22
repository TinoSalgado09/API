using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ArticleDb>(opt => opt.UseInMemoryDatabase("ArticleList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/articles", async (ArticleDb db) =>
    await db.Articles.ToListAsync());

app.MapGet("/artciles/available", async (ArticleDb db) =>
    await db.Articles.Where(t => t.Stock).ToListAsync());

app.MapGet("/article/{id}", async (int id, ArticleDb db) =>
    await db.Articles.FindAsync(id)
        is Article article
            ? Results.Ok(article)
            : Results.NotFound());

app.MapPost("/article", async (Article article, ArticleDb db) =>
{
    db.Articles.Add(article);
    await db.SaveChangesAsync();

    return Results.Created($"/article/{article.Id}", article);
});

app.MapPut("/article/{id}", async (int id, Article inputArticle, ArticleDb db) =>
{
    var article = await db.Articles.FindAsync(id);

    if (article is null) return Results.NotFound();

    article.Name = inputArticle.Name;
    article.Price = inputArticle.Price;
    article.Stock = inputArticle.Stock;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/article/{id}", async (int id, ArticleDb db) =>
{
    if (await db.Articles.FindAsync(id) is Article article)
    {
        db.Articles.Remove(article);
        await db.SaveChangesAsync();
        return Results.Ok(article);
    }

    return Results.NotFound();
});

app.Run();

class Article
{
    public int Id { get; set; }
    public string? Name { get; set; }

    public double Price { get; set; }

    public bool Stock { get; set; }
}

class ArticleDb : DbContext
{
    public ArticleDb(DbContextOptions<ArticleDb> options)
        : base(options) { } 

    public DbSet<Article> Articles => Set<Article>();
}