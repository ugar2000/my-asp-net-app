export interface ClubSessionParticipantDto {
  displayName: string;
  isLeader: boolean;
}

export interface ClubSessionStateDto {
  sessionId: string;
  codeDocument: string;
  consoleOutput: string;
  participants: ClubSessionParticipantDto[];
}

export interface ClubSessionUpdateDto {
  sessionId: string;
  editorDelta: string;
  outputAppend?: string | null;
  author: string;
}
