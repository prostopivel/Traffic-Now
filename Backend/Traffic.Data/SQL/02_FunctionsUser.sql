create or replace function create_unique_email_user()
returns trigger as $$
begin
	if exists (
		select 1 from Users
		where Email = new.Email
	) then
		raise exception 'Email % already exists!', new.Email;
	end if;
	
	return new;
end;
$$ language plpgsql;

create or replace trigger unique_email_user_trigger
	before insert on Users
	for each row
	execute function create_unique_email_user();

create or replace function get_user_by_email(UserEmail varchar(100))
returns table (
	Id uuid,
	Email varchar(100),
	Password varchar(100),
	IsAdmin boolean
) as $$
begin
	return query select * from Users
	where Email = UserEmail;
end;
$$ language plpgsql;

create or replace function create_user(
	UserId uuid,
	UserEmail varchar(100),
	UserPassword varchar(100),
	UserIsAdmin boolean)
returns uuid as $$
begin
	insert into Users (Id, Email, Password, IsAdmin)
	values (UserId, UserEmail, UserPassword, UserIsAdmin);
	
	return UserId;
end;
$$ language plpgsql;

create or replace function update_user(
	UserId uuid,
	UserEmail varchar(100),
	UserPassword varchar(100))
returns uuid as $$
begin
	update Users
	set Email = UserEmail, Password = UserPassword
	where Id = UserId;
	
	return UserId;
end;
$$ language plpgsql;

create or replace function delete_user(UserId uuid)
returns uuid as $$
begin
	delete from Users
	where Id = UserId;
	
	return UserId;
end;
$$ language plpgsql;






