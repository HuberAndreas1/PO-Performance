import {
  Component,
  input,
  effect,
  ElementRef,
  viewChild,
  ChangeDetectionStrategy,
} from '@angular/core';
import {
  Chart,
  LineController,
  LineElement,
  PointElement,
  LinearScale,
  CategoryScale,
  Title,
  Tooltip,
  Legend,
  Filler,
} from 'chart.js';

// Register Chart.js components
Chart.register(
  LineController,
  LineElement,
  PointElement,
  LinearScale,
  CategoryScale,
  Title,
  Tooltip,
  Legend,
  Filler,
);

export interface ChartDataPoint {
  date: string;
  value: number;
}

@Component({
  selector: 'app-cyberlift-chart',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './cyberlift-chart.html',
  styleUrl: './cyberlift-chart.css',
})
export class CyberLiftChartComponent {
  /** 1RM progression data points */
  oneRepMaxData = input<ChartDataPoint[]>([]);

  /** Max weight progression data points */
  maxWeightData = input<ChartDataPoint[]>([]);

  /** Dates where a plateau was detected (ISO date strings) */
  highlightPlateaus = input<string[]>([]);

  private ormCanvas = viewChild<ElementRef<HTMLCanvasElement>>('ormChart');
  private weightCanvas = viewChild<ElementRef<HTMLCanvasElement>>('weightChart');

  private ormChart: Chart | null = null;
  private weightChart: Chart | null = null;

  constructor() {
    // React to input changes and redraw charts
    effect(() => {
      const ormData = this.oneRepMaxData();
      const weightData = this.maxWeightData();
      const plateaus = this.highlightPlateaus();
      const ormEl = this.ormCanvas();
      const weightEl = this.weightCanvas();

      if (ormEl && ormData.length > 0) {
        this.renderChart(
          ormEl.nativeElement,
          'orm',
          '1RM Progression',
          ormData,
          plateaus,
          'rgba(59, 130, 246, 1)',
          'rgba(59, 130, 246, 0.1)',
        );
      }

      if (weightEl && weightData.length > 0) {
        this.renderChart(
          weightEl.nativeElement,
          'weight',
          'Max Weight Progression',
          weightData,
          plateaus,
          'rgba(16, 185, 129, 1)',
          'rgba(16, 185, 129, 0.1)',
        );
      }
    });
  }

  private renderChart(
    canvas: HTMLCanvasElement,
    type: 'orm' | 'weight',
    title: string,
    data: ChartDataPoint[],
    plateaus: string[],
    lineColor: string,
    fillColor: string,
  ): void {
    // Destroy existing chart if it exists
    const existing = type === 'orm' ? this.ormChart : this.weightChart;
    if (existing) {
      existing.destroy();
    }

    const labels = data.map((d) => d.date);
    const values = data.map((d) => d.value);

    // Determine point colors: red for plateau dates, transparent otherwise
    const pointColors = data.map((d) =>
      plateaus.includes(d.date) ? 'rgba(239, 68, 68, 1)' : lineColor,
    );
    const pointRadii = data.map((d) => (plateaus.includes(d.date) ? 6 : 3));

    const chart = new Chart(canvas, {
      type: 'line',
      data: {
        labels,
        datasets: [
          {
            label: title,
            data: values,
            borderColor: lineColor,
            backgroundColor: fillColor,
            fill: true,
            tension: 0.3,
            pointBackgroundColor: pointColors,
            pointBorderColor: pointColors,
            pointRadius: pointRadii,
            pointHoverRadius: 8,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { display: true, position: 'top' },
          tooltip: {
            callbacks: {
              afterLabel: (context) => {
                const date = labels[context.dataIndex];
                return plateaus.includes(date) ? '⚠️ Plateau detected' : '';
              },
            },
          },
        },
        scales: {
          x: {
            title: { display: true, text: 'Date' },
            grid: { color: 'rgba(0,0,0,0.05)' },
          },
          y: {
            title: { display: true, text: type === 'orm' ? '1RM (kg)' : 'Weight (kg)' },
            beginAtZero: false,
            grid: { color: 'rgba(0,0,0,0.05)' },
          },
        },
      },
    });

    if (type === 'orm') {
      this.ormChart = chart;
    } else {
      this.weightChart = chart;
    }
  }
}
