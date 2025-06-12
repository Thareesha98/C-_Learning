using System;

using Microsoft.AspNetCore.Mvc;

using System.Collections.Generic;
using System.Linq;


public class WeatherForecast

{

    public DateTime Date { get; set; }

    public int TemperatureC { get; set; }

    public string? Summary { get; set; }

}

[ApiController]
[Route("[controller]")]

public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]

    {

        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"

    };




    [HttpGet]
    public IEnumerable<WeatherForecast> Get()
    {
        var rng = new Random();
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = rng.Next(-20, 55),
            Summary = Summaries[rng.Next(Summaries.Length)]
        })
        .ToArray();
    }


    [HttpPost]
    public IActionResult Post([FromBody] WeatherForecast forecast)
    {
        return Ok(forecast);
    }

    [HttpPut("{id}")]
    public IActionResult Put(int id, [FromBody] WeatherForecast forecast)
    {
        
        return Ok(forecast);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        
        return NoContent();
    }



}