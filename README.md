# CityBike Data Analysis

A C# console application that analyzes CitiBike trip data and correlates it with historical weather patterns.

## Overview

The project processes large datasets of bike trips (CSV) and weather logs to perform statistical analysis using LINQ. It focuses on identifying high-traffic routes, station imbalances, and the impact of environmental factors (rain, wind, cloud cover) on different user groups (Commuters vs. Tourists).

## Data Sources

The analysis is based on two specific CSV datasets:

1.  **CitiBike Trip Data (June 2022)**
    * File: `JC-202206-citibike-tripdata.csv`

2.  **Historical Weather Data (2016 - 2022)**
    * File: `NYC_Weather_2016_2022.csv`

## Key Features

* **Route Analytics:** Identifies most popular routes and recreational loops.
* **Station Flow:** Calculates net flow (departures vs. arrivals) to identify stations requiring rebalancing.
* **User Segmentation:** Compares trip duration and frequency between Annual Members and Casual users.
* **Weather Correlation:**
    * Normalizes data by hours of occurrence to avoid base rate fallacy.
    * Analyzes ridership intensity during rain, wind, and varying cloud cover.
    * Filters analysis to daylight hours (06:00 - 22:00) for accuracy.
