create or replace function select_point(PointId uuid)
returns table (
	Id uuid,
	MapId uuid,
	X double precision,
	Y double precision,
	Name varchar(100)
) as $$
begin
	return query
	select p.Id, p.MapId, p.X, p.Y, p.Name from Points p
	where p.Id = PointId;
end;
$$ language plpgsql;

create or replace function create_point(
	PointId uuid,
	PointMapId uuid,
	PointX double precision,
	PointY double precision,
	PointName varchar(100))
returns uuid as $$
begin
	insert into Points (Id, MapId, X, Y, Name)
	values (PointId, PointMapId, PointX, PointY, PointName);

	return PointId;
end;
$$ language plpgsql;

create or replace function select_map_points(CurrentMapId uuid)
returns table (
	Id uuid,
	MapId uuid,
	X double precision,
	Y double precision,
	Name varchar(100)
) as $$
begin
	return query
	select p.Id, p.MapId, p.X, p.Y, p.Name from Points p
	where p.MapId = CurrentMapId;
end;
$$ language plpgsql;

create or replace function select_connected_points(PointId uuid)
returns table (
	Id uuid,
	MapId uuid,
	X double precision,
	Y double precision,
	Name varchar(100)
) as $$
begin
	return query
	select distinct p.Id, p.MapId, p.X, p.Y, p.Name from Points p
    join Points_Points pp on p.Id = pp.RightId
    where pp.LeftId = PointId;
end;
$$ language plpgsql;

create or replace function connect_points(
	PointLeftId uuid,
	PointRightId uuid
)
returns uuid as $$
begin
	insert into Points_Points (LeftId, RightId)
	values (PointLeftId, PointRightId);

	return PointLeftId;
end;
$$ language plpgsql;