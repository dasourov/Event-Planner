import * as signalR from '@microsoft/signalr';

export class CommentHubConnection {
  private connection: signalR.HubConnection | null = null;
  private eventId: string;

  constructor(eventId: string) {
    this.eventId = eventId;
  }

  public async start(
    onCommentCreated: (comment: any) => void,
    onCommentUpdated: (comment: any) => void,
    onCommentDeleted: (commentId: string) => void
  ) {
    const token = localStorage.getItem('token');
    
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(token ? `/hubs/comments?access_token=${token}` : '/hubs/comments')
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.connection.on('CommentCreated', (comment: any) => {
      onCommentCreated(comment);
    });

    this.connection.on('CommentUpdated', (comment: any) => {
      onCommentUpdated(comment);
    });

    this.connection.on('CommentDeleted', (commentId: string) => {
      onCommentDeleted(commentId);
    });

    try {
      await this.connection.start();
      console.log('SignalR connected to CommentHub.');
      await this.connection.invoke('JoinEventGroup', this.eventId);
      console.log(`Joined SignalR group for event: ${this.eventId}`);
    } catch (err) {
      console.error('Error starting SignalR connection:', err);
    }
  }

  public async stop() {
    if (this.connection) {
      try {
        await this.connection.invoke('LeaveEventGroup', this.eventId);
        await this.connection.stop();
        console.log('SignalR disconnected.');
      } catch (err) {
        console.error('Error stopping SignalR connection:', err);
      }
    }
  }
}
