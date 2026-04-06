using LinkPara.Calendar.Domain.Entities;
using LinkPara.Calendar.Domain.Enums;
using LinkPara.Calendar.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace LinkPara.Calendar.Infrastructure;

public static class DataSeeder
{
    private const string CountryCode = "TUR";
    private const string CreatedBy = "BATCH";

    public static async Task Initialize(IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetService<CalendarDbContext>();

        if (!context.Holiday.Any())
        {
            var january = new Holiday
            {
                CountryCode = CountryCode,
                Name = "Yeni Yıl Tatili",
                HolidayType = HolidayType.OfficialHoliday,
                CreatedBy = CreatedBy,
                CreateDate = DateTime.Now
            };
            var april = new Holiday
            {
                CountryCode = CountryCode,
                Name = "Ulusal Egemenlik ve Çocuk Bayramı",
                HolidayType = HolidayType.OfficialHoliday,
                CreatedBy = CreatedBy,
                CreateDate = DateTime.Now
            };
            var mayOne = new Holiday
            {
                CountryCode = CountryCode,
                Name = "Emek ve Dayanışma Günü",
                HolidayType = HolidayType.OfficialHoliday,
                CreatedBy = CreatedBy,
                CreateDate = DateTime.Now
            };
            var may = new Holiday
            {
                CountryCode = CountryCode,
                Name = "Atatürk'ü Anma Gençlik ve Spor Bayramı",
                HolidayType = HolidayType.OfficialHoliday,
                CreatedBy = CreatedBy,
                CreateDate = DateTime.Now
            };
            var july = new Holiday
            {
                CountryCode = CountryCode,
                Name = "Demokrasi ve Milli Birlik Günü",
                HolidayType = HolidayType.OfficialHoliday,
                CreatedBy = CreatedBy,
                CreateDate = DateTime.Now
            };
            var agust = new Holiday
            {
                CountryCode = CountryCode,
                Name = "Zafer Bayramı",
                HolidayType = HolidayType.OfficialHoliday,
                CreatedBy = CreatedBy,
                CreateDate = DateTime.Now
            };
            var october = new Holiday
            {
                CountryCode = CountryCode,
                Name = "Cumhuriyet Bayramı",
                HolidayType = HolidayType.OfficialHoliday,
                CreatedBy = CreatedBy,
                CreateDate = DateTime.Now
            };

            for (int i = 2022; i < 2050; i++)
            {
                context.HolidayDetail.Add(new HolidayDetail
                {
                    DateOfHoliday = new DateTime(i, 1, 1),
                    BeginningTime = new DateTime(i, 1, 1, 00, 00, 00),
                    EndingTime = new DateTime(i, 1, 1, 23, 59, 59),
                    DurationInDays = 1,
                    Holiday = january,
                    CreatedBy = CreatedBy,
                    CreateDate = DateTime.Now
                });

                context.HolidayDetail.Add(new HolidayDetail
                {
                    DateOfHoliday = new DateTime(i, 4, 23),
                    BeginningTime = new DateTime(i, 4, 23, 00, 00, 00),
                    EndingTime = new DateTime(i, 4, 23, 23, 59, 59),
                    DurationInDays = 1,
                    Holiday = april,
                    CreatedBy = CreatedBy,
                    CreateDate = DateTime.Now
                });

                context.HolidayDetail.Add(new HolidayDetail
                {
                    DateOfHoliday = new DateTime(i, 5, 1),
                    BeginningTime = new DateTime(i, 5, 1, 00, 00, 00),
                    EndingTime = new DateTime(i, 5, 1, 23, 59, 59),
                    DurationInDays = 1,
                    Holiday = mayOne,
                    CreatedBy = CreatedBy,
                    CreateDate = DateTime.Now
                });

                context.HolidayDetail.Add(new HolidayDetail
                {
                    DateOfHoliday = new DateTime(i, 5, 19),
                    BeginningTime = new DateTime(i, 5, 19, 00, 00, 00),
                    EndingTime = new DateTime(i, 5, 19, 23, 59, 59),
                    DurationInDays = 1,
                    Holiday = may,
                    CreatedBy = CreatedBy,
                    CreateDate = DateTime.Now
                });

                context.HolidayDetail.Add(new HolidayDetail
                {
                    DateOfHoliday = new DateTime(i, 7, 15),
                    BeginningTime = new DateTime(i, 7, 15, 00, 00, 00),
                    EndingTime = new DateTime(i, 7, 15, 23, 59, 59),
                    DurationInDays = 1,
                    Holiday = july,
                    CreatedBy = CreatedBy,
                    CreateDate = DateTime.Now
                });

                context.HolidayDetail.Add(new HolidayDetail
                {
                    DateOfHoliday = new DateTime(i, 8, 30),
                    BeginningTime = new DateTime(i, 8, 30, 00, 00, 00),
                    EndingTime = new DateTime(i, 8, 30, 23, 59, 59),
                    DurationInDays = 1,
                    Holiday = agust,
                    CreatedBy = CreatedBy,
                    CreateDate = DateTime.Now
                });

                context.HolidayDetail.Add(new HolidayDetail
                {
                    DateOfHoliday = new DateTime(i, 10, 29),
                    BeginningTime = new DateTime(i, 10, 28, 12, 00, 01),
                    EndingTime = new DateTime(i, 10, 29, 23, 59, 59),
                    DurationInDays = 1,
                    Holiday = october,
                    CreatedBy = CreatedBy,
                    CreateDate = DateTime.Now
                });
            }

            var religious1 = new Holiday
            {
                CountryCode = CountryCode,
                Name = "Ramazan Bayramı",
                HolidayType = HolidayType.ReligiousHoliday,
                CreatedBy = CreatedBy,
                CreateDate = DateTime.Now
            };
            var religious2 = new Holiday
            {
                CountryCode = CountryCode,
                Name = "Kurban Bayramı",
                HolidayType = HolidayType.ReligiousHoliday,
                CreatedBy = CreatedBy,
                CreateDate = DateTime.Now
            };

            context.HolidayDetail.Add(new HolidayDetail
            {
                DateOfHoliday = new DateTime(2022, 5, 2),
                BeginningTime = new DateTime(2022, 5, 1, 12, 00, 01),
                EndingTime = new DateTime(2022, 5, 4, 23, 59, 59),
                DurationInDays = 3,
                Holiday = religious1,
                CreatedBy = CreatedBy,
                CreateDate = DateTime.Now
            });

            context.HolidayDetail.Add(new HolidayDetail
            {
                DateOfHoliday = new DateTime(2023, 4, 21),
                BeginningTime = new DateTime(2023, 4, 20, 12, 00, 01),
                EndingTime = new DateTime(2023, 4, 23, 23, 59, 59),
                DurationInDays = 3,
                Holiday = religious1,
                CreatedBy = CreatedBy,
                CreateDate = DateTime.Now
            });

            context.HolidayDetail.Add(new HolidayDetail
            {
                DateOfHoliday = new DateTime(2024, 4, 10),
                BeginningTime = new DateTime(2024, 4, 9, 12, 00, 01),
                EndingTime = new DateTime(2024, 4, 12, 23, 59, 59),
                DurationInDays = 3,
                Holiday = religious1,
                CreatedBy = CreatedBy,
                CreateDate = DateTime.Now
            });

            context.HolidayDetail.Add(new HolidayDetail
            {
                DateOfHoliday = new DateTime(2022, 7, 9),
                BeginningTime = new DateTime(2022, 7, 8, 12, 00, 01),
                EndingTime = new DateTime(2022, 7, 12, 23, 59, 59),
                DurationInDays = 4,
                Holiday = religious2,
                CreatedBy = CreatedBy,
                CreateDate = DateTime.Now
            });

            context.HolidayDetail.Add(new HolidayDetail
            {
                DateOfHoliday = new DateTime(2023, 6, 28),
                BeginningTime = new DateTime(2023, 6, 27, 12, 00, 01),
                EndingTime = new DateTime(2023, 7, 1, 23, 59, 59),
                DurationInDays = 4,
                Holiday = religious2,
                CreatedBy = CreatedBy,
                CreateDate = DateTime.Now
            });

            context.HolidayDetail.Add(new HolidayDetail
            {
                DateOfHoliday = new DateTime(2024, 6, 16),
                BeginningTime = new DateTime(2024, 6, 15, 12, 00, 01),
                EndingTime = new DateTime(2024, 6, 19, 23, 59, 59),
                DurationInDays = 4,
                Holiday = religious2,
                CreatedBy = CreatedBy,
                CreateDate = DateTime.Now
            });
            
            await context.SaveChangesAsync();
        }
    }
}