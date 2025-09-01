create table if not exists Users (
	Id uuid PRIMARY KEY,
	Email varchar(100) unique,
	Password varchar(100),
	IsAdmin Boolean
);

create table if not exists Maps (
	Id uuid PRIMARY KEY,
	Name varchar(100)
);

create table if not exists Points (
	Id uuid PRIMARY KEY,
	MapId uuid,
	X double precision,
	Y double precision,
	Name varchar(100),
	foreign key(MapId) references Maps(Id) on delete cascade
);

create table if not exists Transport (
	Id uuid PRIMARY KEY,
	UserId uuid,
	PointId uuid,
	foreign key(UserId) references Users(Id) on delete cascade,
	foreign key(PointId) references Points(Id) on delete cascade
);

create table if not exists Routes (
	Id uuid PRIMARY KEY,
	TransportId uuid,
	RouteTime timestamp,
	foreign key(TransportId) references Transport(Id) on delete cascade
);

create table if not exists Points_Points (
	LeftId uuid,
	RightId uuid,
	primary key(LeftId, RightId),
	foreign key(LeftId) references Points(Id) on delete cascade,
	foreign key(RightId) references Points(Id) on delete cascade
);

create table if not exists Routes_Points (
	RouteId uuid,
	PointId uuid,
	primary key(RouteId, PointId),
	foreign key(RouteId) references Routes(Id) on delete cascade,
	foreign key(PointId) references Points(Id) on delete cascade
);

create table if not exists Users_Maps (
	UserId uuid,
	MapId uuid,
	primary key(UserId, MapId),
	foreign key(UserId) references Users(Id) on delete cascade,
	foreign key(MapId) references Maps(Id) on delete cascade
);

create index if not exists UserEmailIndex on Users (Email);