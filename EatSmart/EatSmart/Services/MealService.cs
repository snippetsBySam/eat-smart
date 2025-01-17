﻿using EatSmart.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EatSmart.Services
{
    public class MealService : IMealService
    {
        private ISpoonacularService spoonacularService;

        public MealService(ISpoonacularService _spoonacularService)
        {
            spoonacularService = _spoonacularService;
        }

        public List<MealDto> Get3Meals(MealRequest mealRequest)
        {
            var meals = new List<MealDto>();

            var breakFastRequest = SetMealRequest(mealRequest, MealType.Breakfast);
            var breakfastResponse = spoonacularService.GetRecipes(breakFastRequest);
            var breakfast = SetMealResponse(breakfastResponse, MealType.Breakfast);
            meals.Add(breakfast);

            var lunchRequest = SetMealRequest(mealRequest, MealType.MainCourse);
            var lunchResponse = spoonacularService.GetRecipes(lunchRequest);
            var lunch = SetMealResponse(lunchResponse, MealType.Lunch);
            meals.Add(lunch);

            var dinnerRequest = SetMealRequest(mealRequest, MealType.MainCourse);
            var dinnerResponse = spoonacularService.GetRecipes(dinnerRequest);
            MealDto dinner;
            do
            {
                dinner = SetMealResponse(dinnerResponse, MealType.Dinner);
            } while (dinner.Id == lunch.Id);
            meals.Add(dinner);

            return meals;
        }

        private MealRequest SetMealRequest(MealRequest userRequest, MealType mealType)
        {
            var mealRequest = new MealRequest();
            mealRequest.MaxCalories = userRequest.MaxCalories / 3;
            mealRequest.MinCalories = mealRequest.MaxCalories * 0.75;
            mealRequest.Type = mealType;
            mealRequest.Intolerances = userRequest.Intolerances;

            return mealRequest;
        }

        private MealDto SetMealResponse(MealResponse mealResponse, MealType mealType)
        {
            MealDto meal = new MealDto();
            var availableMeals = mealResponse.Meals.ToList();
            if (availableMeals.Count() > 0)
            {
                meal = ConvertToMealDto(GetRandomMeal(availableMeals));
                meal.Type = mealType;
            }
            return meal;
        }

        private Meal GetRandomMeal(List<Meal> meals)
        {
            var rand = new Random();
            return meals.Any() ? meals.ElementAt(rand.Next(meals.Count())) : null;
        }

        private MealDto ConvertToMealDto(Meal mealModel)
        {
            var nutrients = mealModel.Nutrition?.Nutrients;
            var mealDto = new MealDto()
            {
                Id = mealModel.Id,
                Title = mealModel.Title,
                Calories = SetNutritionDetails(nutrients, "Calories"),
                Carbs = SetNutritionDetails(nutrients, "Carbohydrates"),
                Fat = SetNutritionDetails(nutrients, "Fat"),
                Protein = SetNutritionDetails(nutrients, "Protein"),
            };

            return mealDto;
        }

        private string SetNutritionDetails(IEnumerable<Nutrient> nutrients, string nutrientType)
        {
            if(nutrients == null || nutrients.Count()==0)
            {
                return null;
            }
            var nutrient = nutrients.SingleOrDefault(n => n.Name == nutrientType);
            return Math.Round((nutrient?.Amount ?? 0)).ToString() + nutrient?.Unit;
        }
    }
}
