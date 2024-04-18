﻿using SkySensorsAPI.Models.Dto;
using SkySensorsAPI.Models.Infrastructure;
using SkySensorsAPI.Repositories;
using System.Net.NetworkInformation;

namespace SkySensorsAPI.ApplicationServices;

public interface ITimeSlotAppService
{
	public Task<TimeSlotDto> UpsertTimeSlot(PhysicalAddress macAddress);
}

public class TimeSlotAppService(
	ITimeSlotRepository timeSlotRepository) : ITimeSlotAppService
{
	public async Task<TimeSlotDto> UpsertTimeSlot(PhysicalAddress macAddress)
	{
		// Find the time schedule that would fit for this device
		// Check if schedule already exists for this device
		TimeSlot? timeSlot = await timeSlotRepository.GetMacAddressTimeSlot(macAddress);

		// IF timeslot already exists, then return it
		if (timeSlot != null)
		{
			return new TimeSlotDto { SecondsNumber = timeSlot.SecondsNumber };
		}

		// If not, then find the least used timeslot
		int secondsNumber = await timeSlotRepository.GetBestTimeSlot();

		await timeSlotRepository.InsertTimeSlot(macAddress, secondsNumber);

		return new TimeSlotDto { SecondsNumber = secondsNumber };
	}
}