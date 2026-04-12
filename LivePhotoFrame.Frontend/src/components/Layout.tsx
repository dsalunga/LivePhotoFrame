import { NavLink, Outlet } from 'react-router-dom';

function Layout() {
  return (
    <>
      <nav className="nav">
        <div className="container">
          <NavLink to="/" className="brand">LivePhotoFrame</NavLink>
          <NavLink to="/" end className={({ isActive }) => isActive ? 'active' : ''}>
            Home
          </NavLink>
          <NavLink to="/counter" className={({ isActive }) => isActive ? 'active' : ''}>
            Counter
          </NavLink>
          <NavLink to="/fetchdata" className={({ isActive }) => isActive ? 'active' : ''}>
            Fetch Data
          </NavLink>
        </div>
      </nav>
      <main className="container">
        <Outlet />
      </main>
    </>
  );
}

export default Layout;
