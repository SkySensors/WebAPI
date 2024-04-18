﻿using Microsoft.AspNetCore.Mvc;
using SkySensorsAPI.ApplicationServices;
using SkySensorsAPI.Models.DTO;

namespace SkySensorsAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherStationController(
	ITimeSlotAppService timeSlotAppService,
	IWeatherStationAppService weatherStationAppService) : ControllerBase
{
	[HttpGet]
	public async Task<ActionResult<List<WeatherStationDTO>>> GetWeatherStation(string macAddress, long startTime = 1713260957000, long endTime = 1713260957000)
	{
		return new List<WeatherStationDTO>()
		{
			await weatherStationAppService.GetWeatherStation(macAddress, startTime, endTime)
		};
	}

	[HttpGet("all")]
	public async Task<ActionResult<List<WeatherStationDTO>>> GetAllWeatherStations(long startTime = 1713260957000, long endTime = 1713260957000)
	{
		List<WeatherStationDTO> weatherStations = await weatherStationAppService.GetWeatherStations(startTime, endTime);
		return weatherStations == null ? NotFound() : Ok(weatherStations);
	}

	[HttpPost]
	public async Task<IActionResult> AddSensorValues(MeasuredSensorValuesDTO[] measuredSensorValuesDTOs)
	{
		await weatherStationAppService.InsertMeasuredSensorValues(measuredSensorValuesDTOs);
		return Ok();
	}

	[HttpPost("handshake")]
	public async Task<ActionResult<TimeSlotDTO>> MakeWeatherStationHandshake(WeatherStationBasicDTO weatherStation)
	{
		// Insert or update weather station
		await weatherStationAppService.UpsertWeatherStation(weatherStation);

		// Add all sensors that does not exist
		foreach (var sensor in weatherStation.Sensors)
		{
			await weatherStationAppService.UpsertWeatherStationSensor(weatherStation.MacAddress, sensor.Type.ToString());
		}

		// Find the time schedule that would fit for this device
		TimeSlotDTO timeSlotDTO = await timeSlotAppService.UpsertTimeSlot(weatherStation.MacAddress);

		return timeSlotDTO;
	}

	[HttpGet("list")]
	public async Task<ActionResult<WeatherStationLocationAndMacDTO>> GetAllLocationsAndMacAddresses()
	{
		IEnumerable<WeatherStationLocationAndMacDTO> weatherStations = await weatherStationAppService.GetWeatherStationLists();
		return weatherStations == null ? NotFound() : Ok(weatherStations);
	}
}