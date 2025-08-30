create or replace function select_route(RouteId uuid)
returns table (
	Id uuid,
	TransportId uuid,
	RouteTime timestamp
) as $$
begin
	return query
	select r.Id, r.TransportId, r.RouteTime from Routes r
	where r.Id = RouteId;
end;
$$ language plpgsql;

create or replace function select_user_routes(CurrentUserId uuid)
returns table (
	Id uuid,
	TransportId uuid,
	RouteTime timestamp
) as $$
begin
	return query 
	select r.Id, r.TransportId, r.RouteTime from Routes r
	join Transport t on t.Id = r.TransportId
	join Users u on u.Id = t.UserId
	where t.UserId = CurrentUserId;
end;
$$ language plpgsql;

create or replace function create_route(
	RouteId uuid,
	RouteTransportId uuid,
	RouteRouteTime timestamp)
returns uuid as $$
begin
	insert into Routes (Id, TransportId, RouteTime)
	values (RouteId, RouteTransportId, RouteRouteTime);

	return RouteId;
end;
$$ language plpgsql;

create or replace function select_route_points(CurrentRouteId uuid)
returns table (
	Id uuid,
	RouteId uuid,
	X double precision,
	Y double precision,
	Name varchar(100)
) as $$
begin
	return query
	select p.Id, p.MapId, p.X, p.Y, p.Name from Points p
	join Routes_Points rp on rp.PointId = p.Id
	where rp.RouteId = CurrentRouteId;
end;
$$ language plpgsql;

create or replace function create_route_points(CurrentRouteId uuid, CurrentPointId uuid)
returns uuid as $$
begin
	insert into Routes_Points (RouteId, PointId)
	values (CurrentRouteId, CurrentPointId);

	return CurrentRouteId;
end;
$$ language plpgsql;