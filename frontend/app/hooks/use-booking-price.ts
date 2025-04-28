import { DateTime } from "luxon";

export const calculateBookingPrice = (
  startDate: Date,
  endDate: Date,
  monthlyPrice: number
): number => {
  const start = DateTime.fromJSDate(startDate);
  const end = DateTime.fromJSDate(endDate);

  // Check if it's exactly one full calendar month
  const isStartOfMonth = start.hasSame(start.startOf("month"), "day");
  const isEndOfMonth = end.hasSame(end.endOf("month"), "day");
  const isSameMonthAndYear =
    start.hasSame(end, "month") && start.hasSame(end, "year");

  if (isStartOfMonth && isEndOfMonth && isSameMonthAndYear) {
    // Exactly one full calendar month: return base price in cents
    console.log("Calculating as full month");
    return Math.round(monthlyPrice * 100);
  } else {
    // Otherwise, prorate based on days using the approximate daily rate
    console.log("Calculating prorated price");
    // Calculate inclusive number of days
    const diffInDays = end.diff(start, "days").as("days");
    const numberOfDays = Math.round(diffInDays) + 1; // Add 1 for inclusivity

    const dailyRate = monthlyPrice / 30; // Approximate daily rate
    const totalPriceFloat = Math.max(dailyRate * numberOfDays, dailyRate); // Ensure min price
    return Math.round(totalPriceFloat * 100); // Return cents
  }
};
