import { Component } from '@angular/core';
import { CyberLiftChartComponent } from '../cyberlift-chart/cyberlift-chart';

// TODO: Implement the Dashboard component.
//
// Hints:
//   - Use HttpClient to call GET /api/exercises and GET /api/exercises/{id}/history
//   - Use Angular signals to store the fetched data
//   - Define interfaces for ExerciseOverviewDto and ExerciseHistoryDto (see README)
//   - Import DatePipe and DecimalPipe from '@angular/common' for the template pipes
//   - Use the CyberLiftChartComponent (already imported) to display progression charts
//     It accepts inputs: [oneRepMaxData], [maxWeightData], [highlightPlateaus]
//     See cyberlift-chart.ts for the ChartDataPoint interface
//   - Set the API base URL from environment.development.ts via ApiConfiguration

@Component({
  selector: 'app-dashboard',
  imports: [CyberLiftChartComponent],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class DashboardComponent {}
