import { Component, OnInit, ElementRef, ViewChildren, QueryList, AfterViewChecked, ViewChild } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Chart, registerables } from 'chart.js/auto';

export interface ChartDataPoint {
  label: string;
  value: number;
}

export interface ChartSeries {
  name: string;
  data: ChartDataPoint[];
}

export interface ChartDto {
  chartType: string;
  title: string;
  series: ChartSeries[];
}

export interface SourceLink {
  title: string;
  url: string;
  type: string; // "web" | "database" | "document"
  snippet: string;
}

export interface InsightsResponse {
  insights: string;
  charts: ChartDto[];
  sources: string[];
  dataSource: string; // "Database" | "WebSearch" | "Mixed" | "Knowledge"
  sourceLinks: SourceLink[];
}

export interface ConversationMessage {
  role: 'user' | 'assistant';
  content: string;
}

export interface ChatMessage {
  role: 'user' | 'assistant';
  text: string;
  response?: InsightsResponse;
}

@Component({
  selector: 'app-chat',
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.scss']
})
export class ChatComponent implements OnInit, AfterViewChecked {
  question: string = '';
  isLoading = false;
  error: string | null = null;
  messages: ChatMessage[] = [];
  private chartInstances: Map<string, Chart> = new Map();
  private shouldScrollToBottom = false;

  @ViewChildren('chartCanvas') chartCanvases!: QueryList<ElementRef<HTMLCanvasElement>>;
  @ViewChild('messagesEnd') messagesEnd!: ElementRef;

  constructor(private http: HttpClient) {
    Chart.register(...registerables);
  }

  ngOnInit(): void {}

  ngAfterViewChecked(): void {
    if (this.shouldScrollToBottom) {
      this.scrollToBottom();
      this.shouldScrollToBottom = false;
    }
    this.renderAllCharts();
  }

  askQuestion(): void {
    const q = this.question.trim();
    if (!q || this.isLoading) return;

    this.messages.push({ role: 'user', text: q });
    this.question = '';
    this.isLoading = true;
    this.error = null;
    this.shouldScrollToBottom = true;

    const history: ConversationMessage[] = this.messages
      .slice(0, -1)
      .map(m => ({ role: m.role, content: m.role === 'assistant' ? (m.response?.insights ?? m.text) : m.text }));

    this.http.post<InsightsResponse>('/api/ai/analyze', { question: q, history }).subscribe({
      next: (data) => {
        this.messages.push({ role: 'assistant', text: data.insights, response: data });
        this.isLoading = false;
        this.shouldScrollToBottom = true;
      },
      error: (err) => {
        console.error('API Error:', err);
        const apiMsg: string = err?.error?.error ?? err?.error?.detail ?? null;
        this.error = apiMsg
          ? `Error: ${apiMsg}`
          : `Request failed (HTTP ${err?.status ?? '?'}). Check that the backend and Ollama API are running.`;
        this.isLoading = false;
      }
    });
  }

  getMessageId(index: number): string {
    return `msg-${index}`;
  }

  hasSourceLinks(response?: InsightsResponse): boolean {
    return !!(response?.sourceLinks?.length);
  }

  webLinks(response?: InsightsResponse): SourceLink[] {
    return response?.sourceLinks?.filter(l => l.type === 'web' && l.url) ?? [];
  }

  internalLinks(response?: InsightsResponse): SourceLink[] {
    return response?.sourceLinks?.filter(l => l.type !== 'web') ?? [];
  }

  getDomain(url: string): string {
    try { return new URL(url).hostname.replace(/^www\./, ''); } catch { return url; }
  }

  getFavicon(url: string): string {
    try {
      const domain = new URL(url).hostname;
      return `https://icons.duckduckgo.com/ip3/${domain}.ico`;
    } catch { return ''; }
  }

  dataSourceLabel(source: string): string {
    const map: Record<string, string> = {
      Database: 'Database',
      WebSearch: 'Web Search',
      Mixed: 'Database + Web',
      Knowledge: 'AI Knowledge'
    };
    return map[source] ?? source;
  }

  dataSourceClass(source: string): string {
    const map: Record<string, string> = {
      Database: 'badge-db',
      WebSearch: 'badge-web',
      Mixed: 'badge-mixed',
      Knowledge: 'badge-knowledge'
    };
    return map[source] ?? 'badge-db';
  }

  private scrollToBottom(): void {
    try {
      this.messagesEnd?.nativeElement?.scrollIntoView({ behavior: 'smooth' });
    } catch {}
  }

  private renderAllCharts(): void {
    const canvases = this.chartCanvases?.toArray() ?? [];
    canvases.forEach((canvasRef) => {
      const canvas = canvasRef.nativeElement;
      const canvasId = canvas.id;
      if (!canvasId || this.chartInstances.has(canvasId)) return;

      const parts = canvasId.split('-');
      const msgIdx = parseInt(parts[1]);
      const chartIdx = parseInt(parts[2]);
      const chartData = this.messages[msgIdx]?.response?.charts?.[chartIdx];
      if (!chartData || chartData.chartType === 'table') return;

      const ctx = canvas.getContext('2d');
      if (!ctx) return;

      const labels = chartData.series[0]?.data.map(d => d.label) ?? [];
      const datasets = chartData.series.map(s => ({
        label: s.name,
        data: s.data.map(d => d.value),
        borderWidth: 1
      }));

      let type: any = 'bar';
      if (chartData.chartType === 'line') type = 'line';
      if (chartData.chartType === 'pie') type = 'pie';

      const chart = new Chart(ctx, {
        type,
        data: { labels, datasets },
        options: {
          responsive: true,
          plugins: {
            title: { display: !!chartData.title, text: chartData.title }
          }
        }
      });
      this.chartInstances.set(canvasId, chart);
    });
  }
}
