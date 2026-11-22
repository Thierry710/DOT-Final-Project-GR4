namespace TraceLinq.Data.Models;

public enum UserRole { Admin, Producer, Inspector, Warehouse }

public enum BatchStatus { Registered, Passed, Failed, InStorage, Dispatched }

public enum QualityResult { Passed, Failed }

public enum StorageStatus { InStorage, Dispatched }
