import * as signalR from '@microsoft/signalr';

export type CommentHubConnectionState = 'connecting' | 'connected' | 'disconnected' | 'reconnecting';

export class CommentHubConnection {
  private connection: signalR.HubConnection | null = null;
  private eventId: string;
  private joined = false;

  constructor(eventId: string) {
    this.eventId = eventId;
  }

  public async start(
    onCommentCreated: (comment: any) => void,
    onCommentUpdated: (comment: any) => void,
    onCommentDeleted: (commentId: string) => void,
    onConnectionStateChange?: (state: CommentHubConnectionState) => void
  ) {
    if (this.connection) {
      await this.stop();
    }

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/comments', {
        accessTokenFactory: () => localStorage.getItem('token') ?? '',
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.ServerSentEvents | signalR.HttpTransportType.LongPolling,
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    this.connection.on('CommentCreated', onCommentCreated);
    this.connection.on('CommentUpdated', onCommentUpdated);
    this.connection.on('CommentDeleted', onCommentDeleted);

    this.connection.onreconnecting(() => {
      this.joined = false;
      onConnectionStateChange?.('reconnecting');
    });

    this.connection.onreconnected(async () => {
      try {
        await this.joinGroup();
        onConnectionStateChange?.('connected');
      } catch (err) {
        console.error('Error rejoining SignalR group after reconnect:', err);
        onConnectionStateChange?.('disconnected');
      }
    });

    this.connection.onclose(() => {
      this.joined = false;
      onConnectionStateChange?.('disconnected');
    });

    try {
      onConnectionStateChange?.('connecting');
      await this.connection.start();
      await this.joinGroup();
      onConnectionStateChange?.('connected');
    } catch (err) {
      console.error('Error starting SignalR connection:', err);
      onConnectionStateChange?.('disconnected');
    }
  }

  private async joinGroup() {
    if (!this.connection || this.joined) return;
    await this.connection.invoke('JoinEventGroup', this.eventId);
    this.joined = true;
  }

  public async stop() {
    if (!this.connection) return;

    try {
      if (this.joined) {
        await this.connection.invoke('LeaveEventGroup', this.eventId);
        this.joined = false;
      }
      await this.connection.stop();
    } catch (err) {
      console.error('Error stopping SignalR connection:', err);
    } finally {
      this.connection = null;
    }
  }
}
