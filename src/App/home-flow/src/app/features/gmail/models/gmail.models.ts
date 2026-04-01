export interface GmailConnection {
  gmailConnectionId: string;
  tenantId: string;
  householdId: string;
  googleEmail: string;
  status: string;
  connectedAtUtc: string;
}

export interface AppointmentSuggestion {
  sourceMessageId: string;
  sourceMessageDateUtc: string;
  sender: string;
  subject: string;
  sourceSnippet: string;
  sourceBody: string;
  sourceAttachmentNames: string[];
  sourceAttachmentText: string;
  suggestedTitle: string;
  suggestedStartsAtUtc: string | null;
  suggestedEndsAtUtc: string | null;
  suggestedLocation: string | null;
  suggestedType: string;
  suggestedDescription: string | null;
  suggestedAmount: number | null;
  confidence: number;
  reason: string;
}

export interface ScanGmailSuggestionsResponse {
  householdId: string;
  fromUtc: string;
  toUtc: string;
  suggestions: AppointmentSuggestion[];
}
