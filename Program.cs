using EmployeeTimeSheetAPI;
using EmployeeTimeSheetAPI.Entities;
using EmployeeTimeSheetAPI.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System.Net.WebSockets;

var builder = WebApplication.CreateBuilder(args);

var conString = builder.Configuration.GetConnectionString("EmployeeTimeSheetDb") ?? "DataSource = Timesheet.db";
builder.Services.AddSqlite<EmployeeTimeSheetDb>(conString);


builder.Services.AddDbContext<EmployeeTimeSheetDb>(opt => opt.UseInMemoryDatabase("EmployeeList"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var empItems = app.MapGroup("/employees");
    
var employeeLists = new List<Employee> {
    new Employee(){ ID = 1, Name = "Jame" },
    new Employee(){ ID = 2, Name = "Jone" },
    new Employee(){ ID = 3, Name = "Smith" }
};

//Read all employees
empItems.MapGet("/", GetAllEmployees);

//Read employee by id  
empItems.MapGet("/{id}", GetEmployeeById);

//Add new employee
empItems.MapPost("/", CreateEmployee);

//Update Employee
empItems.MapPut("/", UpdateEmployee);

//Delete the student
empItems.MapDelete("/{id}", DeleteEmployee);


//Run the Rest API Server at Port: 7192, change in launchSetting.json
app.Run();

static async Task<IResult> GetAllEmployees(EmployeeTimeSheetDb db) {
    return TypedResults.Ok(await db.Employees.ToArrayAsync());
}
static async Task<IResult> GetEmployeeById(int id, EmployeeTimeSheetDb db) {
    var emp = await db.Employees.FindAsync(id);
    return emp == null ? TypedResults.NotFound() : TypedResults.Ok(emp);
} 
static async Task<IResult> CreateEmployee([FromBody] Employee inputEmp, EmployeeTimeSheetDb db) {
    var emp = db.Employees.Find(inputEmp.ID);
    if (emp != null)  { return  TypedResults.Ok("id already used"); }
    db.Employees.Add(inputEmp);
    await db.SaveChangesAsync();
    return TypedResults.Created($"/employees/{inputEmp}", inputEmp);
}
static async Task<IResult> UpdateEmployee([FromBody] Employee inputEmp, EmployeeTimeSheetDb db) {
    if (inputEmp is null) { return TypedResults.NotFound(); }
    var emp = await db.Employees.FindAsync(inputEmp.ID);
    if (emp is null) { return TypedResults.NotFound(); }
    
    emp.Name = inputEmp.Name;
    await db.SaveChangesAsync();

    return TypedResults.NoContent();
}

static async Task<IResult> DeleteEmployee(int? id, EmployeeTimeSheetDb db) {
    if (id is null) return TypedResults.NotFound();

    var emp = await db.Employees.FindAsync(id);
    if (emp is null) { return TypedResults.NotFound(); }

    db.Employees.Remove(emp);
    await db.SaveChangesAsync();
    return TypedResults.Ok(id);
}

