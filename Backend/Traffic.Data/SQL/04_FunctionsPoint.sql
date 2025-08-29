create or replace function select_point(PointId uuid)
returns table (
	Id uuid,
	MapId uuid,
	X double precision,
	Y double precision,
	Name varchar(100)
) as $$
begin
	return query select * from Points p
	where p.Id = PointId;
end;
$$ language plpgsql;

create or replace function create_point(
	PointId uuid,
	MapId uuid,
	PointX double precision,
	PointY double precision,
	PointName varchar(100))
returns uuid as $$
begin
	insert into Points (Id, MapId, X, Y, Name)
	values (PointId, MapId, PointX, PointY, PointName);

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
	return query select * from Points p
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
	return query select distinct * from Points p
    join Points_Points pp on p.Id = pp.RightId
    where pp.LeftId = PointId;
end;
$$ language plpgsql;







